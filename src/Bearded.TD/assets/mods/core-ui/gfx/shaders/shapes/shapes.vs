#version 400

uniform mat4 projection;
uniform mat4 view;

in vec3 v_position;

in int v_shapeType1Flags1;
in vec4 v_shapeData;
in vec3 v_shapeData2;

in uint v_firstComponent;
in int v_componentCount;

out vec3 p_position;
out vec2 p_screenUV;
flat out int p_shapeType1Flags1;
flat out vec4 p_shapeData;
flat out vec3 p_shapeData2;

flat out uint p_firstComponent;
flat out int p_componentCount;

void main()
{
	gl_Position = projection * view * vec4(v_position, 1.0);
	p_screenUV = (gl_Position.xy / gl_Position.w) * 0.5 + 0.5;

	p_position = v_position;
	p_shapeType1Flags1 = v_shapeType1Flags1;
	p_shapeData = v_shapeData;
	p_shapeData2 = v_shapeData2;
	
	p_firstComponent = v_firstComponent;
	p_componentCount = v_componentCount;
}
