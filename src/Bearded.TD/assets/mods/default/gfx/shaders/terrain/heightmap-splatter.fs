#version 150

uniform sampler2D splat;

in vec2 fragmentUV;
in float fragmentMinHeight;
in float fragmentMaxHeight;

out vec4 fragColor;

void main()
{
	vec4 c = texture(splat, fragmentUV);
	
	float height = mix(fragmentMinHeight, fragmentMaxHeight, c.r);

    fragColor = vec4(height, height, height, 1) * c.a;
}
