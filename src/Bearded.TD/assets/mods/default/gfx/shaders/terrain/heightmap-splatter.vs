#version 150

uniform float heightmapRadius;

in vec2 vertexPosition;
in vec2 vertexUV;
in float vertexMinHeight;
in float vertexMaxHeight;

out vec2 fragmentUV;
out float fragmentMinHeight;
out float fragmentMaxHeight;

void main()
{
	vec2 p = vertexPosition / heightmapRadius; // -1..1

	gl_Position = vec4(p, 0, 1.0);

	fragmentUV = vertexUV;
	fragmentMinHeight = vertexMinHeight;
	fragmentMaxHeight = vertexMaxHeight;
}