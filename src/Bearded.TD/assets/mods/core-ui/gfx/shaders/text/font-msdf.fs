#version 150

uniform vec2 unitRange;
uniform sampler2D msdf;

in vec4 p_color;
in vec2 p_texcoord;

out vec4 fragColor;

float screenPixelRange(vec2 uv) {
    vec2 screenTextureSize = vec2(1.0) / fwidth(uv);
    return max(0.5 * dot(unitRange, screenTextureSize), 1.0);
}

float median(float r, float g, float b) {
    return max(min(r, g), min(max(r, g), b));
}

float pixelDistanceAt(vec2 uv) {
    vec3 distances = texture(msdf, uv).rgb;
    float distance = median(distances.r, distances.g, distances.b);
    float pixelDistance = screenPixelRange(uv) * (distance - 0.5);
    return pixelDistance;
}

#define MULTISAMPLE true

void main() {
    float pixelDistance;

    float multiSampleThreshold = 2;
    float multiSampleStrength = multiSampleThreshold - dot(unitRange, vec2(1.0) / fwidth(p_texcoord));

    if (MULTISAMPLE && multiSampleStrength > 0)
    {
        vec2 dx = dFdx(p_texcoord);
        vec2 dy = dFdy(p_texcoord);
        vec2 uvOffsets = vec2(0.125, 0.375) * min(multiSampleStrength, 1);
        vec2 offsetUV = vec2(0.0, 0.0);
        float accumulatedPixelDistance = 0;
        offsetUV.xy = p_texcoord + uvOffsets.x * dx + uvOffsets.y * dy;
        accumulatedPixelDistance += pixelDistanceAt(offsetUV);
        offsetUV.xy = p_texcoord - uvOffsets.x * dx - uvOffsets.y * dy;
        accumulatedPixelDistance += pixelDistanceAt(offsetUV);
        offsetUV.xy = p_texcoord + uvOffsets.y * dx - uvOffsets.x * dy;
        accumulatedPixelDistance += pixelDistanceAt(offsetUV);
        offsetUV.xy = p_texcoord - uvOffsets.y * dx + uvOffsets.x * dy;
        accumulatedPixelDistance += pixelDistanceAt(offsetUV);

        pixelDistance = accumulatedPixelDistance * 0.25;
    }
    else
    {
        pixelDistance = pixelDistanceAt(p_texcoord);
    }
    
    
    float opacity = clamp(pixelDistance + 0.4, 0.0, 1.0);
    
    opacity = smoothstep(0.0, 1.0, opacity);

    fragColor = p_color * opacity;
}
