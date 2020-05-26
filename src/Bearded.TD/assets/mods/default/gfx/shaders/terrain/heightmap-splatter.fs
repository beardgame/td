#version 150

uniform sampler2D splat;

in float fragmentHeight;
in vec2 fragmentUV;

out vec4 fragColor;

void main()
{
	vec4 c = texture(splat, fragmentUV);

    fragColor = vec4(fragmentHeight, 0, 0, 1) * c.r;
}