#version 150

uniform mat4 projection;
uniform mat4 view;

in vec3 vertexPosition;
in vec3 vertexNormal;
in vec2 vertexUV;
in vec4 vertexColor;

out vec3 fragmentNormal;
out vec2 fragmentUV;
out vec4 fragmentColor;
out float fragmentDepth;

void main()
{
    gl_Position = projection * view * vec4(vertexPosition, 1.0);
    fragmentNormal = vertexNormal;
    fragmentUV = vertexUV;
    fragmentColor = vertexColor;
    fragmentDepth = vertexPosition.z;
}
