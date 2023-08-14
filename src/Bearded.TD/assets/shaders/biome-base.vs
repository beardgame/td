#version 150

uniform float biomemapRadius;

in vec3 vertexPosition;
in float biomeId;

out float geometryBiomeId;

void main()
{
	vec2 p = vertexPosition.xy / biomemapRadius; // -1..1

	gl_Position = vec4(p, 0, 1.0);

	geometryBiomeId = uint(round(biomeId));
}
