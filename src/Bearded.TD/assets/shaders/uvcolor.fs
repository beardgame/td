#version 150

uniform sampler2D diffuse;

in vec4 p_color;
in vec2 p_texcoord;

out vec4 fragColor;

void main()
{
	vec4 c = p_color * texture(diffuse, p_texcoord);
    fragColor = c;
}