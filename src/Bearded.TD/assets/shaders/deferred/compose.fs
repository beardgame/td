#version 150

uniform sampler2D albedoTexture;
uniform sampler2D lightTexture;

in vec2 fragmentUV;

out vec4 outColor;

void main()
{
    vec4 albedo = texture(albedoTexture, fragmentUV);
    vec3 lightTexture = texture(lightTexture, fragmentUV).rgb;

    vec3 rgb = albedo.rgb * (lightTexture + vec3(0.1, 0.1, 0.1)) + vec3(0.1, 0.1, 0.1);

    outColor = vec4(rgb, albedo.a);
}
