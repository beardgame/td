#version 150

uniform sampler2D caveTexture;
uniform mat4 projection;
uniform mat4 view;

in vec3 v_position;
in vec2 v_texcoord;
in vec4 v_color;

out vec2 p_position;
out vec2 p_backgroundUV;
out vec4 p_backgroundColor;

void main()
{
	gl_Position = projection * view * vec4(v_position, 1.0);

	vec2 imageSize = textureSize(caveTexture, 0);

	p_position = (v_texcoord - 0.5);
	p_position *= imageSize.x / imageSize.y;
	p_backgroundUV = p_position;
	p_backgroundUV.x *= imageSize.y / imageSize.x;
	p_backgroundUV += 0.5;
	
	p_backgroundColor = v_color;
}
