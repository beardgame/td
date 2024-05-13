#version 150

uniform mat4 projection;
uniform mat4 view;

uniform float farPlaneDistance;

in vec3 v_position;
in vec2 v_texcoord;
in vec4 v_color;

out vec3 fragmentPosition;
out vec2 fragmentUV;
out vec4 fragmentColor;
out float fragmentDepth;


void main()
{
	vec4 viewPosition = view * vec4(v_position, 1.0);
	vec4 position = projection * viewPosition;
	gl_Position = position;

	fragmentColor = v_color;
	fragmentUV = v_texcoord;
	fragmentDepth = -viewPosition.z / farPlaneDistance;
	fragmentPosition = v_position;
}
