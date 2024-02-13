#version 150

uniform mat4 projection;
uniform mat4 view;

in vec3 v_position;
in vec4 v_color;

in int v_shapeType;
in vec4 v_shapeData;
in vec4 v_edgeData;

out vec4 p_color;

out vec3 p_position;
flat out int p_shapeType;
flat out vec4 p_shapeData;
flat out vec4 p_edgeData;

void main()
{
	gl_Position = projection * view * vec4(v_position, 1.0);
	p_color = v_color;

	p_position = v_position;
	p_shapeType = v_shapeType;
	p_shapeData = v_shapeData;
	p_edgeData = v_edgeData;
}
