#version 150

uniform float heightmapRadius;

in vec3 v_position;

out float f_height;

void main()
{
	vec2 p = v_position.xy / heightmapRadius; // -1..1

	gl_Position = vec4(p, 0, 1.0);

	f_height = v_position.z;
}
