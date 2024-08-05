#version 410

uniform vec2 gridScale;

in vec3 vertexPosition;
in vec4 vertexColor;

in vec2 instanceOffset;

out vec2 vertexPositionTCS;
out vec4 vertexColorTCS;

void main()
{
    vertexPositionTCS = vertexPosition.xy * gridScale + instanceOffset;
    vertexColorTCS = vertexColor;
}
