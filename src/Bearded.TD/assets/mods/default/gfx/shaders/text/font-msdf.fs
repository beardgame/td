#version 150

uniform vec2 unitRange;
uniform sampler2D msdf;

in vec4 p_color;
in vec2 p_texcoord;

out vec4 fragColor;

float screenPixelRange() {
    vec2 screenTextureSize = vec2(1.0) / fwidth(p_texcoord);
    return max(0.5 * dot(unitRange, screenTextureSize), 1.0);
}

float median(float r, float g, float b) {
    return max(min(r, g), min(max(r, g), b));
}

void main() {
    vec3 distances = texture(msdf, p_texcoord).rgb;
    float distance = median(distances.r, distances.g, distances.b);
    float pixelDistance = screenPixelRange() * (distance - 0.5);
    
    float opacity = clamp(pixelDistance + 0.5, 0.0, 1.0);

    fragColor = p_color * opacity;
}
