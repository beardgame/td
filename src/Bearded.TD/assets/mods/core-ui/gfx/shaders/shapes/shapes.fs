#version 400

const float PI = 3.14159265359;
const float TAU = 6.28318530718;

const int SHAPE_TYPE_FILL = 0;
const int SHAPE_TYPE_LINE = 1; // point to point
const int SHAPE_TYPE_CIRCLE = 2; // center and radius
const int SHAPE_TYPE_RECTANGLE = 3; // top-left, width, height, corner radius
const int SHAPE_TYPE_HEXAGON = 4; // center and radius
const int SHAPE_TYPE_HEXGRID = 5; // origin tile and 4x4 bitfield

const int SHAPE_FLAG_PROJECT_ON_DEPTHBUFFER = 1;

const int GRADIENT_TYPE_NONE = 0;
const int GRADIENT_TYPE_CONSTANT = 1;
const int GRADIENT_BLURRED_BACKGROUND = 2;

const int GRADIENT_TYPE_LINEAR = 20; // point to point
const int GRADIENT_TYPE_RADIAL_RADIUS = 21; // center and radius
const int GRADIENT_TYPE_RADIAL_POINT_ON_EDGE = 22; // center and point on edge
const int GRADIENT_TYPE_ALONG_EDGE_NORMAL = 23;
const int GRADIENT_ARC_AROUND_POINT = 24; // center, start and total angle in radians

const uint GRADIENT_FLAG_GLOWFADE = 1;
const uint GRADIENT_FLAG_DITHER = 2;
const uint GRADIENT_FLAG_EXTEND_NEGATIVE = 4;
const uint GRADIENT_FLAG_EXTEND_POSITIVE = 8;
const uint GRADIENT_FLAG_REPEAT = 16;

const float ANTI_ALIAS_WIDTH = 1;

uniform usamplerBuffer gradientBuffer;
uniform usamplerBuffer componentBuffer;

uniform sampler2D intermediateBlurBackground;

uniform sampler2D depthBuffer;
uniform vec3 farPlaneBaseCorner;
uniform vec3 farPlaneUnitX;
uniform vec3 farPlaneUnitY;
uniform vec3 cameraPosition;

uniform float uiTime;

in vec3 p_position;
in vec2 p_screenUV;

flat in int p_shapeType1Flags1;
flat in vec4 p_shapeData;
flat in vec3 p_shapeData2;
flat in vec4 p_edgeData;

flat in uint p_firstComponent;
flat in int p_componentCount;

out vec4 fragColor;

vec3 getFragmentPositionFromDepth(vec2 uv)
{
    uv = clamp(uv, 0.001, 0.999);

    float depth = texture(depthBuffer, uv).x;

    vec3 pointOnFarPlane = farPlaneBaseCorner
        + farPlaneUnitX * uv.x
        + farPlaneUnitY * uv.y;

    vec3 fragmentPositionRelativeToCamera = pointOnFarPlane * depth;
    vec3 fragmentPosition = fragmentPositionRelativeToCamera - cameraPosition;

    return fragmentPosition;
}

float smoothMerge(float d1, float d2, float r){
    vec2 intersectionSpace = vec2(d1 - r, d2 - r);
    intersectionSpace = min(intersectionSpace, 0);
    float insideDistance = -length(intersectionSpace);
    float simpleUnion = min(d1, d2);
    float outsideDistance = max(simpleUnion, r);
    return insideDistance + outsideDistance;
}

float hexDistance(vec2 p, float radius, float cornerRadius, float innerGlowRoundness, float centerRoundness)
{
    const vec3 k = vec3(-0.866025404, 0.5, 0.577350269);
    float d = length(p);
    float cornersToCenter = 1 - clamp(d / radius * -k.x, 0, 1);
    float roundness = mix(innerGlowRoundness, centerRoundness, cornersToCenter);
    cornerRadius += radius * roundness * cornersToCenter;
    cornerRadius = min(cornerRadius, radius);

    float r = radius - cornerRadius;
    vec2 d2 = abs(p.yx);
    d2 -= 2.0 * min(dot(k.xy, d2), 0.0) * k.xy;
    d2 -= vec2(clamp(d2.x, -k.z * r, k.z * r), r);
    float hexDistance = length(d2) * sign(d2.y);

    return hexDistance - cornerRadius;
}

float signedDistanceToEdge(vec2 p0, int type)
{
    switch(type) {
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
        case SHAPE_TYPE_HEXAGON:
        {
            vec2 center = p_shapeData.xy;
            float cornerR = p_shapeData.w;
            float radius = p_shapeData.z;
            vec2 p = p0 - center;
            
            float innerGlowRoundness = p_shapeData2.x + 0.5;
            float centerRoundness = p_shapeData2.y + 0.5;

            return hexDistance(p, radius, cornerR, innerGlowRoundness, centerRoundness);
        }
        case SHAPE_TYPE_HEXGRID:
        {
            ivec2 originTile = floatBitsToInt(p_shapeData.xy);
            int bitfield = floatBitsToInt(p_shapeData.z);
            
            float cornerR = p_shapeData.w;
            float innerGlowRoundness = p_shapeData2.x + 0.5;
            float centerRoundness = p_shapeData2.y + 0.5;
            
            float distance = 1.0 / 0;
            
            for (int i = 0; i < 16; i++)
            {
                if ((bitfield & (1 << i)) == 0)
                    continue;
                
                int x = i % 4;
                int y = i / 4;

                ivec2 tile = originTile + ivec2(x, y);
                vec2 center = vec2(tile.x + tile.y * 0.5, tile.y * 0.866025404);
                
                float d = hexDistance(p0 - center, 0.5, cornerR, innerGlowRoundness, centerRoundness);
                
                distance = smoothMerge(distance, d, cornerR);
            }
            return distance;
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
                clamp((t - current.position) / (next.position - current.position), 0, 1)
            );
        }
        current = next;
    }

    return stopColor(current);
}
vec4 getColorInGradient(uint index, float t, float tapWidth, int taps)
{
    vec4 color = vec4(0);
    float width = tapWidth / taps;
    float min = t - tapWidth / 2;
    for (int i = 0; i < taps; i++)
    {
        color += getColorInGradient(index, min + i * width);
    }
    return color / taps;
}

float hash(float n)
{
    return fract(sin(n) * 43758.5453);
}
float hash(vec2 p)
{
    return fract(sin(dot(p, vec2(11.9898, 78.233))) * 43758.5453);
}

vec4 dither(vec4 color, vec2 seed)
{
    float n = hash(seed) * hash(seed.yx * 12.3) - 0.5;
    
    vec4 added = color.rgba + (n / 128);
    vec4 multiplied = color.rgba * (1 + n / 4); 
    
    return mix(multiplied, added, color.a);
}

vec4 getBackgroundBlur()
{
    vec2 size = textureSize(intermediateBlurBackground, 0);

    float pixelStepY = 1.5 / size.y;
    
    const float[] stepSizes = float[](
    -7, -6, -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5, 6, 7
    );

    const float[] gaussianWeights = float[](
    0.00874088, 0.0179975 , 0.03315999, 0.05467158, 0.0806592,
    0.10648569, 0.12579798, 0.13298454, 0.12579798, 0.10648569,
    0.0806592 , 0.05467158, 0.03315999, 0.0179975 , 0.00874088
    );

    vec4 color = vec4(0);
    
    vec2 offset = vec2(0.5) / size;

    for (int i = 0; i < 15; i++)
    {
        vec2 uv = vec2(p_screenUV.x, p_screenUV.y + stepSizes[i] * pixelStepY) + offset;
        color += texture(intermediateBlurBackground, uv) * gaussianWeights[i];
    }

    return color;
}

float repeat(float t)
{
    return fract(t);
}

vec4 getColor(vec4 parameters, float t, uint type, uint gradientIndex, uint flags)
{
    switch(type)
    {
        case GRADIENT_TYPE_NONE:
        {
            return vec4(0);
        }
        case GRADIENT_TYPE_CONSTANT:
        {
            return rgbaFloatToVec4(parameters.x);
        }
        case GRADIENT_TYPE_LINEAR:
        {
            vec2 p0 = parameters.xy;
            vec2 p1 = parameters.zw;
            vec2 d = p1 - p0;
            float projection = dot(d, p_position.xy - p0) / dot(d, d);
            if ((flags & GRADIENT_FLAG_REPEAT) != 0)
            {
                projection = repeat(projection);
            }
            return getColorInGradient(gradientIndex, projection);
        }
        case GRADIENT_TYPE_RADIAL_RADIUS:
        {
            vec2 center = parameters.xy;
            float radius = parameters.z;
            float distance = length(p_position.xy - center) / radius;
            if ((flags & GRADIENT_FLAG_REPEAT) != 0)
            {
                distance = repeat(distance);
            }
            return getColorInGradient(gradientIndex, distance);
        }
        case GRADIENT_TYPE_RADIAL_POINT_ON_EDGE:
        {
            vec2 center = parameters.xy;
            vec2 pointOnEdge = parameters.zw;
            float radius = length(pointOnEdge - center);
            float distance = length(p_position.xy - center) / radius;
            if ((flags & GRADIENT_FLAG_REPEAT) != 0)
            {
                distance = repeat(distance);
            }
            return getColorInGradient(gradientIndex, distance);
        }
        case GRADIENT_TYPE_ALONG_EDGE_NORMAL:
        {
            if ((flags & GRADIENT_FLAG_REPEAT) != 0)
            {
                t = repeat(t);
            }
            return getColorInGradient(gradientIndex, t);
        }
        case GRADIENT_ARC_AROUND_POINT:
        {
            vec2 center = parameters.xy;
            float startAngle = parameters.z;
            float totalAngle = parameters.w;
            if (totalAngle < 0)
            {
                totalAngle = -totalAngle;
                startAngle -= totalAngle;
            }
            vec2 d = p_position.xy - center;
            float angle = atan(d.y, d.x);
            float normalizedAngle = mod(angle + startAngle, TAU) / totalAngle;
            float distance = length(d);
            if ((flags & GRADIENT_FLAG_REPEAT) != 0)
            {
                normalizedAngle = repeat(normalizedAngle);
            }
            return getColorInGradient(gradientIndex, normalizedAngle, 0.25 / distance, 8);
        }
        case GRADIENT_BLURRED_BACKGROUND:
        {
            return getBackgroundBlur();
        }
    }
    return vec4(1, 0, 1, 0.5);
}

struct BitField
{
    uint data;
};

uint getType(BitField bits)
{
    return bits.data & 0xFFu;
}
uint getFlags(BitField bits)
{
    return (bits.data >> 8) & 0xFFFFu;
}
uint getBlendMode(BitField bits)
{
    return (bits.data >> 24) & 0xFFu;
}

vec4 getColor(BitField bits, uint gradientId, vec4 parameters, float t)
{
    uint type = getType(bits);
    uint flags = getFlags(bits);
    
    vec4 color = getColor(parameters, t, type, gradientId, flags);
    
    if ((flags & GRADIENT_FLAG_GLOWFADE) != 0)
    {
        color = color * smoother(1 - t);
    }
    if ((flags & GRADIENT_FLAG_DITHER) != 0)
    {
        color = dither(color, p_position.xy + hash(uiTime));
    }
    
    return color;
}

struct Contribution
{
    float alpha;
    float zeroToOne;
};

float antiAlias(float distance, float antiAliasWidth)
{
    return clamp(distance / antiAliasWidth + 0.5, 0, 1);
}

Contribution contributionOf(float zero, float one, float distance, float antiAliasWidth, BitField bits)
{
    float zeroToOne = clamp((distance - zero) / (one - zero), 0, 1);

    uint flags = getFlags(bits);
    bool extendNegative = (flags & GRADIENT_FLAG_EXTEND_NEGATIVE) != 0;
    bool extendPositive = (flags & GRADIENT_FLAG_EXTEND_POSITIVE) != 0;
    
    float lower = min(zero, one);
    float upper = max(zero, one);

    float lowerAlpha = extendNegative ? 1 : antiAlias(distance - lower, antiAliasWidth);
    float upperAlpha = extendPositive ? 1 : antiAlias(upper - distance, antiAliasWidth);
    
    float alpha = min(lowerAlpha, upperAlpha);
    
    return Contribution(alpha, zeroToOne);
}

vec4 addPremultiplied(vec4 a, vec4 b)
{
    return a * (1 - b.a) + b;
}

const uint BLEND_MODE_PREMULTIPLIEDADD = 0;
const uint BLEND_MODE_MULTIPLY = 1;

vec4 blend(vec4 a, vec4 b, BitField bits)
{
    uint blendMode = getBlendMode(bits);
    switch(blendMode)
    {
        case BLEND_MODE_PREMULTIPLIEDADD:
        {
            return addPremultiplied(a, b);
        }
        case BLEND_MODE_MULTIPLY:
        {
            return a * b;
        }
    }
    return vec4(1, 0, 1, 0.5);
}

vec4 getFragmentColor()
{
    vec4 ret = vec4(0);
    
    vec2 xy = p_position.xy;

    int type = p_shapeType1Flags1 & 0xFF;
    int flags = (p_shapeType1Flags1 >> 8) & 0xFF;
    
    if ((flags & SHAPE_FLAG_PROJECT_ON_DEPTHBUFFER) != 0)
    {
        vec3 projected = getFragmentPositionFromDepth(p_screenUV);
        if (projected.z > p_position.z)
        {
            // geometry behind level, project onto it
            xy = projected.xy;
        }
    }

    float signedDistance = signedDistanceToEdge(xy, type);
    float antiAliasWidth = ANTI_ALIAS_WIDTH * fwidth(signedDistance) * 1;
    
    for(int i = 0; i < p_componentCount; i++)
    {
        int bufferIndex = int(p_firstComponent + i) * 2;
        uvec4 texel1 = texelFetch(componentBuffer, bufferIndex);
        uvec4 texel2 = texelFetch(componentBuffer, bufferIndex + 1);
        
        float zeroDistance = uintBitsToFloat(texel1.x);
        float oneDistance = uintBitsToFloat(texel1.y);
        BitField bits = BitField(texel1.z);
        uint gradientId = texel1.w;
        
        vec4 gradientParameters = uintBitsToFloat(texel2);

        Contribution contribution = contributionOf(zeroDistance, oneDistance, signedDistance, antiAliasWidth, bits);

        vec4 color = getColor(bits, gradientId, gradientParameters, contribution.zeroToOne);
        
        ret = blend(ret, color * contribution.alpha, bits);
    }
    
    return ret;
}

const float DEBUG_SHAPE = 0;

void main()
{
    fragColor = getFragmentColor();

    if (DEBUG_SHAPE != 0)
    {
        float signedDistance = signedDistanceToEdge(p_position.xy, p_shapeType1Flags1 & 0xFF);
        float a = 0.5 + cos(signedDistance * 3.14) * 0.3;
        vec2 xy = abs(fract(p_position.xy * vec2(0.2)) - vec2(0.5));
        float d = length(xy);
        
        vec4 c;
        float s;
        if (signedDistance < 0)
        {
            c = vec4(1, 0, 0, 1);
            s = smoothstep(0.1, 0.0, xy.y) * smoothstep(0.45, 0.4, d);
        }
        else if (signedDistance > 0)
        {
            c = vec4(0, 1, 0, 1);
            s = smoothstep(0.1, 0.0, min(xy.x, xy.y)) * smoothstep(0.45, 0.4, d);
        }
        else
        {
            c = vec4(0, 0, 1, 1);
            s = smoothstep(0.1, 0.0, abs(d - 0.35));
        }
        fragColor = mix(fragColor, c * a + vec4(1) * clamp(s, 0, 1), DEBUG_SHAPE);
    }
}
