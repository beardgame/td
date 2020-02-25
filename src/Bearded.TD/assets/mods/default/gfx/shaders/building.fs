#version 150

uniform sampler2D diffuse;

in vec2 fragmentUV;
in vec4 fragmentColor;
in float fragmentDepth;

out vec4 outRGBA;
out vec4 outNormal;
out vec4 outDepth;

void main()
{
	vec4 rgba = fragmentColor * texture(diffuse, fragmentUV);
	vec3 normal = normalize(vec3(0.5, 0.5, 1));

    outRGBA = rgba;
    outNormal = vec4(normal, 1) * rgba.a;
    outDepth = vec4(fragmentDepth, 0, 0, 1) *  rgba.a;
}