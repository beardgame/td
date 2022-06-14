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
	vec4 rgba = texture(diffuse, fragmentUV);

	if (rgba.a < 0.01)
		discard;

    outRGBA = rgba;
    outNormal = vec4(0.5, 0.5, 1, 1) * rgba.a;
    outDepth = vec4(fragmentDepth, 0, 0, 1) *  rgba.a;
}
