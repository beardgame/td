#version 150

uniform sampler2D splat;

in vec2 fragmentUV;
in float fragmentMinHeight;
in float fragmentMaxHeight;

out vec4 fragColor;

void main()
{
	vec4 c = texture(splat, fragmentUV);

	float r = c.a == 0 ? 0 : (c.r / c.a);

    fragColor = vec4(
    	mix(fragmentMinHeight, fragmentMaxHeight, r) * c.a,
    	0, 0, c.a
    	);
}