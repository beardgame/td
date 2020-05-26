#version 150

uniform float heightmapRadius;

in vec3 v_position;
in vec2 v_texcoord;
in vec4 v_color;

out float fragmentHeight;
out vec2 fragmentUV;

void main()
{
	vec2 p = v_position.xy / heightmapRadius; // -1..1

	gl_Position = vec4(p, 0, 1.0);

	fragmentHeight = v_position.z;
	fragmentUV = v_texcoord;
}