#version 150

const vec2 coreCenter = vec2(0.035, 0.14);
const vec2 coreSize = vec2(0.45, 0.45);

const vec2 smokeCenter = vec2(0.035, 0.14);
const vec2 smokeSize = vec2(0.45, 0.45);

const vec2 smokeCenter2 = vec2(0.05, 0.18);
const vec2 smokeSize2 = vec2(0.58);

const vec4 coreOverlay = vec4(228, 212, 255, 255) / 255 * 0.7;
const vec4 backgroundColor = vec4(0, 3, 13, 255) / 255 * 1.05;

uniform sampler2D caveTexture;
uniform sampler2D coreTexture;
uniform sampler2D smokeTexture;
uniform sampler2D smokeTexture2;

uniform float uiTime;

in vec2 p_position;
in vec2 p_backgroundUV;
in vec4 p_backgroundColor;

out vec4 fragColor;

float hash(float n)
{
    return fract(sin(n) * 43758.5453);
}
float hash(vec2 p)
{
    return fract(sin(dot(p, vec2(11.9898, 78.233))) * 43758.5453);
}

float noise(vec2 x)
{
    vec2 p = floor(x);
    vec2 f = fract(x);

    f = f * f * (3.0 - 2.0 * f);

    float n = p.x + p.y * 57.0;

    float ret = mix(
        mix(hash(n + 0.0),  hash(n + 1.0),  f.x),
        mix(hash(n + 57.0), hash(n + 58.0), f.x),
        f.y);
    
    return ret;
}
float fbm(vec2 uv)
{
    float weight = 0;
    float sum = 0.0;
    for (int i = 0; i <4; ++i) {
        float f = i + 1;
        sum += noise(uv * f) / f;
        weight += 1 / f;
    }
    return sum / weight;
}

void dither(vec2 seed)
{
    float n = (hash(seed) * 2 - 1) * (hash(seed.yx * 12.3) * 2 - 1);
    n /= 128;
    fragColor.rgb += n;
}

void blendEdge()
{
    vec2 v = (p_backgroundUV - vec2(0.5)) * 2;
    float distanceFromCenter = length(pow(abs(v), vec2(2)));
    float edgeGradient = smoothstep(0.75, 1, distanceFromCenter);
    fragColor = mix(fragColor, backgroundColor, edgeGradient);
}

vec4 screen(vec4 a, vec4 b)
{
    return 1 - (1 - a) * (1 - b);
}

vec4 softLight(vec4 a, vec4 b)
{
    return (1 - 2 * b) * a * a + 2 * b * a;
}

vec2 uvOf(vec2 center, vec2 size)
{
    return (p_position - center + size / 2) / size;
}

vec4 tex(sampler2D sampler, vec2 center, vec2 size)
{
    return texture(sampler, uvOf(center, size));
}

void tonemap()
{
    fragColor = (fragColor - vec4(0.03)) * 1.05;
}

vec4 multiSample(sampler2D sampler, vec2 uv, float lod, float multiSampleStrength)
{
    vec2 dx = dFdx(uv);
    vec2 dy = dFdy(uv);
    vec2 uvOffsets = vec2(0.125, 0.375) * multiSampleStrength;
    vec2 offsetUV = vec2(0.0, 0.0);
    vec4 accumulated = vec4(0);
    offsetUV = uv + uvOffsets.x * dx + uvOffsets.y * dy;
    accumulated += texture(sampler, offsetUV, lod);
    offsetUV = uv - uvOffsets.x * dx - uvOffsets.y * dy;
    accumulated += texture(sampler, offsetUV, lod);
    offsetUV = uv + uvOffsets.y * dx - uvOffsets.x * dy;
    accumulated += texture(sampler, offsetUV, lod);
    offsetUV = uv - uvOffsets.y * dx + uvOffsets.x * dy;
    accumulated += texture(sampler, offsetUV, lod);

    return accumulated * 0.25;
}

vec4 sampleCave()
{
    float distanceFromCenter = length(p_backgroundUV - vec2(0.5, 0.6));
    float edgeBlur = 7;//(sin(uiTime*2) * 0.5 + 0.5) * 10;
    float centerBlur = -0.5;
    float lod = mix(centerBlur, edgeBlur, distanceFromCenter);
    
    float shakeStrength = (sin(uiTime*5) * 0.5 + 0.5) * 10;
    float shake = shakeStrength * hash(uiTime); // add to multiSampleStrength
    
    return multiSample(caveTexture, p_backgroundUV, lod, pow(2, lod));
}

vec2 twist(float frequency, float strength, vec2 velocity, vec2 seed)
{
    vec2 tOffset = velocity * uiTime;
    float a = fbm(seed * frequency + tOffset);
    float b = fbm(seed * frequency - 0.5 * tOffset + vec2(0.12, 1.3));
    float twistAmount = (a - 0.5) * 0.2;
    float twistAngle = (b - 0.5) * (a - 0.5) * 100 * strength;
    return vec2(cos(twistAngle), sin(twistAngle)) * twistAmount;
}

vec4 sampleCore()
{
    vec2 smokeUV = uvOf(smokeCenter, smokeSize);
    
    float velocity = 0.05;
    
    vec2 twistOffset = twist(3, 0.75, vec2(0, velocity), smokeUV);
    vec2 twistOffset2 = twist(10, 0.5, vec2(0, -velocity * 2), smokeUV) * 0.5;
    
    vec4 swirl = texture(smokeTexture, smokeUV + twistOffset + twistOffset2, -1);
    
    vec4 smoke = swirl;

    vec4 core = tex(coreTexture, coreCenter, coreSize);

    vec4 ret = core;
    ret = screen(ret, smoke);
    ret = screen(ret, coreOverlay);
    return ret;
}

vec4 sampleCaveSmoke()
{
    vec2 size = smokeSize2;
    vec2 texSize = textureSize(smokeTexture2, 0);
    size.y *= texSize.y / texSize.x;

    vec2 uv = uvOf(smokeCenter2, size);
    
    float velocity = 0.1;
    vec2 twistOffset = twist(5, 0.5, vec2(velocity, 0), uv) * 0.75;
    
    vec4 smoke = texture(smokeTexture2, uv + twistOffset);
    smoke = mix(smoke, vec4(0.5), 1 - smoke.a);
    
    return smoke;
}

void main()
{
    fragColor = sampleCore();
    
    vec4 cave = sampleCave();
    fragColor = mix(fragColor, cave, cave.a);

    blendEdge();

    vec4 caveSmoke = sampleCaveSmoke();
    fragColor = softLight(fragColor, caveSmoke);
    
    tonemap();
    dither(p_backgroundUV + hash(uiTime));
}
