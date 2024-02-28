#version 400

uniform mat4 projection;
uniform mat4 view;

in vec3 v_position;

in int v_shapeType;
in vec4 v_shapeData;
in vec3 v_shapeData2;
in vec4 v_edgeData;

in uint v_fillGradientTypeIndex;
in uint v_edgeGradientTypeIndex;
in uint v_outerGlowGradientTypeIndex;
in uint v_innerGlowGradientTypeIndex;

in vec4 v_fillGradientParameters;
in vec4 v_edgeGradientParameters;
in vec4 v_outerGlowGradientParameters;
in vec4 v_innerGlowGradientParameters;

out vec3 p_position;
flat out int p_shapeType;
flat out vec4 p_shapeData;
flat out vec3 p_shapeData2;
flat out vec4 p_edgeData;

flat out uint p_fillGradientTypeIndex;
flat out uint p_edgeGradientTypeIndex;
flat out uint p_outerGlowGradientTypeIndex;
flat out uint p_innerGlowGradientTypeIndex;

flat out vec4 p_fillGradientParameters;
flat out vec4 p_edgeGradientParameters;
flat out vec4 p_outerGlowGradientParameters;
flat out vec4 p_innerGlowGradientParameters;

void main()
{
	gl_Position = projection * view * vec4(v_position, 1.0);

	p_position = v_position;
	p_shapeType = v_shapeType;
	p_shapeData = v_shapeData;
	p_shapeData2 = v_shapeData2;
	p_edgeData = v_edgeData;
	
	p_fillGradientTypeIndex = v_fillGradientTypeIndex;
	p_edgeGradientTypeIndex = v_edgeGradientTypeIndex;
	p_outerGlowGradientTypeIndex = v_outerGlowGradientTypeIndex;
	p_innerGlowGradientTypeIndex = v_innerGlowGradientTypeIndex;
	
	p_fillGradientParameters = v_fillGradientParameters;
	p_edgeGradientParameters = v_edgeGradientParameters;
	p_outerGlowGradientParameters = v_outerGlowGradientParameters;
	p_innerGlowGradientParameters = v_innerGlowGradientParameters;
}
