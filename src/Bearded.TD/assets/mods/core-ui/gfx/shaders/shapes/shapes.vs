#version 400

const int PART_FILL_I = 0;
const int PART_EDGE_I = 1;
const int PART_GLOW_OUTER_I = 2;
const int PART_GLOW_INNER_I = 3;

uniform mat4 projection;
uniform mat4 view;

in vec3 v_position;

in int v_shapeType;
in vec4 v_shapeData;
in vec3 v_shapeData2;
in vec4 v_edgeData;

in uvec2 v_fillGradientTypeIndexFlags;
in uvec2 v_edgeGradientTypeIndexFlags;
in uvec2 v_outerGlowGradientTypeIndexFlags;
in uvec2 v_innerGlowGradientTypeIndexFlags;

in vec4 v_fillGradientParameters;
in vec4 v_edgeGradientParameters;
in vec4 v_outerGlowGradientParameters;
in vec4 v_innerGlowGradientParameters;

out vec3 p_position;
flat out int p_shapeType;
flat out vec4 p_shapeData;
flat out vec3 p_shapeData2;
flat out vec4 p_edgeData;

flat out uvec2 p_gradientTypeIndicesFlags[4];
flat out vec4 p_gradientParameters[4];

void main()
{
	gl_Position = projection * view * vec4(v_position, 1.0);

	p_position = v_position;
	p_shapeType = v_shapeType;
	p_shapeData = v_shapeData;
	p_shapeData2 = v_shapeData2;
	p_edgeData = v_edgeData;

	p_gradientTypeIndicesFlags[PART_FILL_I] = v_fillGradientTypeIndexFlags;
	p_gradientTypeIndicesFlags[PART_EDGE_I] = v_edgeGradientTypeIndexFlags;
	p_gradientTypeIndicesFlags[PART_GLOW_OUTER_I] = v_outerGlowGradientTypeIndexFlags;
	p_gradientTypeIndicesFlags[PART_GLOW_INNER_I] = v_innerGlowGradientTypeIndexFlags;
	
	p_gradientParameters[PART_FILL_I] = v_fillGradientParameters;
	p_gradientParameters[PART_EDGE_I] = v_edgeGradientParameters;
	p_gradientParameters[PART_GLOW_OUTER_I] = v_outerGlowGradientParameters;
	p_gradientParameters[PART_GLOW_INNER_I] = v_innerGlowGradientParameters;
}
