#version 150

uniform mat4 view;
uniform mat4 projection;

in vec3 vertexPosition;
in vec3 vertexNormal;
in vec4 vertexColor;

out vec3 fragmentPosition;
out vec3 fragmentNormal;
out vec4 fragmentColor;

void main()
{
	vec4 viewPosition = view * vec4(vertexPosition, 1.0);
	vec4 position = projection * viewPosition;
    gl_Position = position;
    
    fragmentPosition = vertexPosition;
    fragmentNormal = vertexNormal;
    fragmentColor = vertexColor;
}
