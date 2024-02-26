#version 150

uniform sampler2D caveTexture;
uniform sampler2D coreTexture;
uniform sampler2D smokeTexture;
uniform sampler2D smokeTexture2;

in vec4 p_color;
in vec2 p_texcoord;

out vec4 fragColor;

void main()
{
	vec4 c = texture(caveTexture, p_texcoord);
    fragColor = c;
}
