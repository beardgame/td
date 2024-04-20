﻿#version 400

const float PI = 3.14159265359;
const float TAU = 6.28318530718;

const int SHAPE_TYPE_FILL = 0;
const int SHAPE_TYPE_LINE = 1; // point to point
const int SHAPE_TYPE_CIRCLE = 2; // center and radius
const int SHAPE_TYPE_RECTANGLE = 3; // top-left, width, height, corner radius
const int SHAPE_TYPE_HEXAGON = 4; // center and radius

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

const float ANTI_ALIAS_WIDTH = 1;

uniform usamplerBuffer gradientBuffer;
uniform usamplerBuffer componentBuffer;

uniform sampler2D intermediateBlurBackground;

uniform float uiTime;

in vec3 p_position;
in vec2 p_screenUV;

flat in int p_shapeType;
flat in vec4 p_shapeData;
flat in vec3 p_shapeData2;
flat in vec4 p_edgeData;

flat in uint p_firstComponent;
flat in int p_componentCount;

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
        case SHAPE_TYPE_HEXAGON:
        {
            const vec3 k = vec3(-0.866025404, 0.5, 0.577350269);
            vec2 center = p_shapeData.xy;
            float cornerR = p_shapeData.w;
            float radius = p_shapeData.z;
            vec2 p = p0 - center;
            
            float innerGlowRoundness = p_shapeData2.x + 0.5;
            float centerRoundness = p_shapeData2.y + 0.5;

            float d = length(p);
            float cornersToCenter = 1 - clamp(d / radius * -k.x, 0, 1);
            float roundness = mix(innerGlowRoundness, centerRoundness, cornersToCenter);
            cornerR += radius * roundness * cornersToCenter;
            cornerR = min(cornerR, radius);
            
            float r = radius - cornerR;
            vec2 d2 = abs(p.yx);
            d2 -= 2.0 * min(dot(k.xy, d2), 0.0) * k.xy;
            d2 -= vec2(clamp(d2.x, -k.z * r, k.z * r), r);
            float hexDistance = length(d2) * sign(d2.y);
            
            return hexDistance - cornerR;
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
    float n = (hash(seed) * 2 - 1) * (hash(seed.yx * 12.3) * 2 - 1);
    n /= 128;
    color.rgba += n;
    return color;
}

vec4 getBackgroundBlur()
{
    vec2 size = textureSize(intermediateBlurBackground, 0);

    float pixelStepY = 1.0 / size.y;

    const float[] gaussianWeights = float[](
    0.00874088, 0.0179975 , 0.03315999, 0.05467158, 0.0806592,
    0.10648569, 0.12579798, 0.13298454, 0.12579798, 0.10648569,
    0.0806592 , 0.05467158, 0.03315999, 0.0179975 , 0.00874088
    );

    vec4 color = vec4(0);

    for (int i = 0; i < 15; i++)
    {
        vec2 uv = vec2(p_screenUV.x, p_screenUV.y + float(i - 7) * pixelStepY);
        color += texture(intermediateBlurBackground, uv) * gaussianWeights[i];
    }

    return color;
}

vec4 getColor(vec4 parameters, float t, uint type, uint gradientIndex)
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
            return getColorInGradient(gradientIndex, projection);
        }
        case GRADIENT_TYPE_RADIAL_RADIUS:
        {
            vec2 center = parameters.xy;
            float radius = parameters.z;
            float distance = length(p_position.xy - center) / radius;
            return getColorInGradient(gradientIndex, distance);
        }
        case GRADIENT_TYPE_RADIAL_POINT_ON_EDGE:
        {
            vec2 center = parameters.xy;
            vec2 pointOnEdge = parameters.zw;
            float radius = length(pointOnEdge - center);
            float distance = length(p_position.xy - center) / radius;
            return getColorInGradient(gradientIndex, distance);
        }
        case GRADIENT_TYPE_ALONG_EDGE_NORMAL:
        {
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
    
    vec4 color = getColor(parameters, t, type, gradientId);
    
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

    float signedDistance = signedDistanceToEdge();
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
        float signedDistance = signedDistanceToEdge();
        float a = 0.5 + cos(signedDistance * 3.14) * 0.3;
        vec2 xy = abs(fract(p_position.xy * vec2(0.2)) - vec2(0.5));
        float d = length(xy);
        
        vec4 c;
        float s;
        if (signedDistance < 0)
        {
            c = vec4(1, 0, 0, 1);
            s = smoothstep(0.1, 0f, xy.y) * smoothstep(0.45, 0.4, d);
        }
        else if (signedDistance > 0)
        {
            c = vec4(0, 1, 0, 1);
            s = smoothstep(0.1, 0f, min(xy.x, xy.y)) * smoothstep(0.45, 0.4, d);
        }
        else
        {
            c = vec4(0, 0, 1, 1);
            s = smoothstep(0.1, 0f, abs(d - 0.35));
        }
        fragColor = mix(fragColor, c * a + vec4(1) * clamp(s, 0, 1), DEBUG_SHAPE);
    }
}
