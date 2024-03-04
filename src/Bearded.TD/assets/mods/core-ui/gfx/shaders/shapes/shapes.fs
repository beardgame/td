#version 400

const int SHAPE_TYPE_FILL = 0;
const int SHAPE_TYPE_LINE = 1; // point to point
const int SHAPE_TYPE_CIRCLE = 2; // center and radius
const int SHAPE_TYPE_RECTANGLE = 3; // top-left, width, height, corner radius

const int GRADIENT_TYPE_NONE = 0;
const int GRADIENT_TYPE_CONSTANT = 1;
const int GRADIENT_TYPE_SIMPLE_GLOW = 2;
const int GRADIENT_TYPE_LINEAR = 20; // point to point
const int GRADIENT_TYPE_RADIAL = 21; // center and radius
const int GRADIENT_TYPE_ALONG_EDGE_NORMAL = 22;

const int EDGE_OUTER_WIDTH_I = 0;
const int EDGE_INNER_WIDTH_I = 1;
const int EDGE_OUTER_GLOW_I = 2;
const int EDGE_INNER_GLOW_I = 3;

const int PART_FILL_I = 0;
const int PART_EDGE_I = 1;
const int PART_GLOW_OUTER_I = 2;
const int PART_GLOW_INNER_I = 3;

const float ANTI_ALIAS_WIDTH = 1;

uniform usamplerBuffer gradientBuffer;

in vec3 p_position;
flat in int p_shapeType;
flat in vec4 p_shapeData;
flat in vec3 p_shapeData2;
flat in vec4 p_edgeData;

flat in uint p_gradientTypeIndices[4];
flat in vec4 p_gradientParameters[4];

out vec4 fragColor;

float signedDistanceToEdge()
{
    vec2 p0 = p_position.xy;
    switch(p_shapeType) {
        case SHAPE_TYPE_LINE:
        {
            vec2 p1 = p_shapeData.xy;
            vec2 p2 = p_shapeData.zw;
            vec2 v12 = p2 - p1;
            vec2 v01 = p1 - p0;
            float numerator = v12.x * v01.y - v12.y * v01.x;
            float denominator = length(v12);
            return numerator / denominator;
        }
        case SHAPE_TYPE_CIRCLE:
        {
            vec2 center = p_shapeData.xy;
            float radius = p_shapeData.z;
            return length(p0 - center) - radius;
        }
        case SHAPE_TYPE_RECTANGLE:
        {
            vec2 topleft = p_shapeData.xy;
            vec2 halfSize = p_shapeData.zw / 2;
            float radius = p_shapeData2.x;
            vec2 center = topleft + halfSize;
            vec2 p = abs(p0 - center);
            vec2 d = p - halfSize + vec2(radius);
            float rectDistance = min(max(d.x, d.y), 0.0) + length(max(d, 0.0)) - radius;

            float cornerTransitionRoundness = p_shapeData2.y;
            float innerGlowRoundness = p_shapeData2.z;

            if (cornerTransitionRoundness == 0 && innerGlowRoundness == 0)
            {
                return rectDistance;
            }
            
            float smallestSize = min(halfSize.x, halfSize.y);
            
            float power = smallestSize / max(max(radius, -rectDistance), rectDistance);
            vec2 power2 = vec2(power);
            if (halfSize.x > halfSize.y)
            {
                power2.x *= halfSize.x / halfSize.y;
            }
            else
            {
                power2.y *= halfSize.y / halfSize.x;
            }
            float ovalDistanceRelative = pow(length(pow(p / halfSize, power2)), 1 / power) - 1;
            float ovalDistance = ovalDistanceRelative * min(halfSize.x, halfSize.y);
            
            float edgeToCenter = clamp(-rectDistance / smallestSize * 2, 0, 1);
            float edgeToOuter = clamp(rectDistance / smallestSize * 2, 0, 1);
            
            float ovalNess = mix(cornerTransitionRoundness, innerGlowRoundness, edgeToCenter);
            ovalNess *= 1 - edgeToOuter;
            
            float mixedDistance = mix(rectDistance, ovalDistance, ovalNess);
            return mixedDistance;
        }
    }
    
    return 0;
}

vec4 rgbaIntToVec4(uint rgba)
{
    return vec4(
    ((rgba >> 0) & 0xFFu) / 255.0,
    ((rgba >> 8) & 0xFFu) / 255.0,
    ((rgba >> 16) & 0xFFu) / 255.0,
    ((rgba >> 24) & 0xFFu) / 255.0
    );
}

vec4 rgbaFloatToVec4(float rgba)
{
    return rgbaIntToVec4(floatBitsToUint(rgba));
}

vec4 getConstantColorFromGradientParameters(int index)
{
    return rgbaFloatToVec4(p_gradientParameters[index].x);
}

float smoother(float x)
{
    return x * x * (3 - 2 * x);
}

struct Stop
{
    uint data;
    float position;
    uint rgba;
};

Stop getStop(uint index)
{
    uvec3 data = texelFetch(gradientBuffer, int(index)).xyz;
    return Stop(
        data.x,
        uintBitsToFloat(data.y),
        data.z
    );
}

vec4 stopColor(Stop stop)
{
    return rgbaIntToVec4(stop.rgba);
}

uint remainingStops(Stop stop)
{
    return stop.data & 0xFFFFu;
}

vec4 getColorInGradient(uint index, float t)
{
    Stop current = getStop(index);
    
    uint otherStopsCount = remainingStops(current);
    
    for (uint i = 1; i < otherStopsCount; i++)
    {
        Stop next = getStop(index + i);
        if (next.position >= t)
        {
            return mix(
                stopColor(current),
                stopColor(next),
                (t - current.position) / (next.position - current.position)
            );
        }
        current = next;
    }

    return stopColor(current);
}

vec4 getColor(int partIndex, float t)
{
    uint typeIndex = p_gradientTypeIndices[partIndex];
    uint type = typeIndex & 0xFFu;
    uint index = typeIndex >> 8;
    
    vec4 parameters = p_gradientParameters[partIndex];
    
    switch(type)
    {
        case GRADIENT_TYPE_CONSTANT:
        {
            return getConstantColorFromGradientParameters(partIndex);
        }
        case GRADIENT_TYPE_SIMPLE_GLOW:
        {
            return getConstantColorFromGradientParameters(partIndex) * smoother(t);
        }
        case GRADIENT_TYPE_LINEAR:
        {
            vec2 p0 = parameters.xy;
            vec2 p1 = parameters.zw;
            vec2 d = p1 - p0;
            float projection = dot(d, p_position.xy - p0) / dot(d, d);
            return getColorInGradient(index, projection);
        }
        case GRADIENT_TYPE_RADIAL:
        {
            vec2 center = parameters.xy;
            float radius = parameters.z;
            float distance = length(p_position.xy - center) / radius;
            return getColorInGradient(index, distance);
        }
        case GRADIENT_TYPE_ALONG_EDGE_NORMAL:
        {
            return getColorInGradient(index, t);
        }
    }
    
    return vec4(1, 0, 1, 0.5);
}

struct Contribution
{
    float alpha;
    float t;
};

struct Separated
{
    vec4 inner;
    vec4 outer;
};

Contribution contributionOf(float fromDistance, float toDistance, float distance, float antiAliasWidth)
{
    float edgeRadius = (toDistance - fromDistance + antiAliasWidth) / 2;
    float edgeCenterOffset = (toDistance + fromDistance) / 2;
    float absoluteDistance = abs(distance - edgeCenterOffset);

    float alpha = clamp((edgeRadius - absoluteDistance) / antiAliasWidth, 0, 1);
    float t = clamp((distance - fromDistance) / (toDistance - fromDistance), 0, 1);
    
    return Contribution(alpha, t);
}

vec4 addPremultiplied(vec4 a, vec4 b)
{
    return a * (1 - b.a) + b;
}

Separated edgeContribution(float distance, float antiAliasWidth)
{
    float edgeOuterWidth = p_edgeData[EDGE_OUTER_WIDTH_I];
    float edgeInnerWidth = p_edgeData[EDGE_INNER_WIDTH_I];
    float edgeOuterGlow = p_edgeData[EDGE_OUTER_GLOW_I];
    float edgeInnerGlow = p_edgeData[EDGE_INNER_GLOW_I];

    Separated ret = Separated(vec4(0), vec4(0));

    if (edgeOuterGlow != 0)
    {
        Contribution glowOuter = contributionOf(edgeOuterWidth, edgeOuterWidth + edgeOuterGlow, distance, antiAliasWidth);
        ret.outer = getColor(PART_GLOW_OUTER_I, 1 - glowOuter.t) * glowOuter.alpha;
    }
    if (edgeInnerGlow != 0)
    {
        Contribution glowInner = contributionOf(-edgeInnerWidth - edgeInnerGlow, -edgeInnerWidth, distance, antiAliasWidth);
        ret.inner = getColor(PART_GLOW_INNER_I, glowInner.t) * glowInner.alpha;
    }

    if (edgeOuterWidth + edgeInnerWidth != 0)
    {
        Contribution edge = contributionOf(-edgeInnerWidth, edgeOuterWidth, distance, antiAliasWidth);
        vec4 e = getColor(PART_EDGE_I, edge.t) * edge.alpha;
        ret.outer = addPremultiplied(ret.outer, e);
        ret.inner = addPremultiplied(ret.inner, e);
    }

    return ret;
}

vec4 fillContribution(float distance, float alpha)
{
    return getColor(PART_FILL_I, 0) * alpha;
}

const float DEBUG_SHAPE = 0;

void main()
{
    fragColor = vec4(0);
    
    if (p_shapeType == SHAPE_TYPE_FILL)
    {
        fragColor = getConstantColorFromGradientParameters(PART_FILL_I);
        if (DEBUG_SHAPE != 0)
        {
            fragColor = mix(fragColor, vec4(0, 0, 1, 1), 0.3);
        }
        return;
    }

    float signedDistance = signedDistanceToEdge();
    float antiAliasWidth = ANTI_ALIAS_WIDTH * length(vec2(dFdx(signedDistance), dFdy(signedDistance)));
    float outsideToInside = clamp((antiAliasWidth / 2 - signedDistance) / antiAliasWidth, 0, 1);
    
    vec4 fill = fillContribution(signedDistance, outsideToInside);
    Separated edges = edgeContribution(signedDistance, antiAliasWidth);
    
    vec4 inner = addPremultiplied(fill, edges.inner);

    fragColor = edges.outer + inner;

    if (DEBUG_SHAPE != 0)
    {
        float a = 0.5 + cos(signedDistance * 3.14) * 0.3;
        if (p_shapeType == SHAPE_TYPE_LINE)
        {
            fragColor = mix(fragColor, vec4(1, 0, 0, 1) * a, DEBUG_SHAPE);
        }
        if (p_shapeType == SHAPE_TYPE_CIRCLE)
        {
            fragColor = mix(fragColor, vec4(0, 1, 0, 1) * a, DEBUG_SHAPE);
        }
        if (p_shapeType == SHAPE_TYPE_RECTANGLE)
        {
            vec2 xy = abs(fract(p_position.xy * vec2(0.2)) - vec2(0.5));
            float d = length(xy);
            
            vec4 c;
            float s;
            if (signedDistance < 0)
            {
                c = vec4(1, 0, 1, 1);
                s = smoothstep(0.1, 0f, xy.y) * smoothstep(0.45, 0.4, d);
            }
            else if (signedDistance > 0)
            {
                c = vec4(1, 1, 0, 1);
                s = smoothstep(0.1, 0f, min(xy.x, xy.y)) * smoothstep(0.45, 0.4, d);
            }
            else
            {
                c = vec4(0, 1, 1, 1);
                s = smoothstep(0.1, 0f, abs(d - 0.35));
            }
            fragColor = mix(fragColor, c * a + vec4(1) * clamp(s, 0, 1), DEBUG_SHAPE);
        }
    }
}
