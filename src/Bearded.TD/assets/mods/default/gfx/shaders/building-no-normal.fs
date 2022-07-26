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

	if (rgba.a < 0.9)
		discard;

    outRGBA = vec4(rgba.rgb * rgba.a, 1);
    outNormal = vec4(0.5, 0.5, 1, 1);
    outDepth = vec4(fragmentDepth, 0, 0, 1);
}
