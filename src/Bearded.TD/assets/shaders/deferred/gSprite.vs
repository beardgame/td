#version 150

uniform mat4 projection;
uniform mat4 view;

in vec3 vertexPosition;
in vec2 vertexUV;
in vec4 vertexColor;

out vec4 fragmentUV;
out vec4 fragmentColor;

void main()
{
    gl_Position = projection * view * vec4(vertexPosition, 1.0);
    fragmentUV = vertexUV;
    fragmentColor = vertexColor;
}
