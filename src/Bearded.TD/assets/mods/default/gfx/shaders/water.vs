#version 150

uniform mat4 projection;
uniform mat4 view;

in vec3 vertexPosition;
in vec3 vertexNormal;
in vec2 vertexFlow;

out vec3 fragmentPosition;
out vec3 fragmentNormal;
out vec2 fragmentFlow;

void main()
{
	vec4 viewPosition = view * vec4(vertexPosition, 1.0);
	vec4 position = projection * viewPosition;
    gl_Position = position;
    
    fragmentPosition = vertexPosition;
    fragmentNormal = vertexNormal;
    fragmentFlow = vertexFlow;
}
