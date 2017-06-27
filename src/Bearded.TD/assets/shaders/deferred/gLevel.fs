#version 150

uniform sampler2D diffuseTexture;
uniform sampler2D normalTexture;

in vec3 fragmentNormal;
in vec2 fragmentUV;
in vec4 fragmentColor;
in float fragmentDepth;

out vec4 outRGBA;
out vec4 outNormal;
out vec4 outDepth;

void main()
{
    vec4 diffuse = vec4(1, 1, 1, 1); //texture(diffuseTexture, fragmentUV);
    vec3 normal = fragmentNormal * 0.5 + 0.5; //texture(normalTexture, fragmentUV).rgb;

    vec4 rgba = diffuse * fragmentColor;

    outRGBA = rgba;
    outNormal = vec4(normal, rgba.a);
    outDepth = vec4(fragmentDepth, 0, 0, rgba.a);
}
