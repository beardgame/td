#version 150

uniform sampler2D diffuse;
uniform sampler2D normal;
uniform sampler2D occlusionMetallicRoughness;

in vec3 fragmentPosition; // ignored here, but used in ghost shader
in vec3 fragmentNormal;
in vec2 fragmentUV;
in float fragmentDepth;

out vec4 outRGBA;
out vec4 outNormal;
out vec4 outDepth;
out vec4 outMaterial;

void main()
{
    vec4 rgba = texture(diffuse, fragmentUV);
    vec3 normal = 0.5 * normalize(fragmentNormal) + 0.5;

    outRGBA = rgba;
    outNormal = vec4(normal, 1);
    outDepth = vec4(fragmentDepth, 0, 0, 1);
    outMaterial = texture(occlusionMetallicRoughness, fragmentUV);
}
