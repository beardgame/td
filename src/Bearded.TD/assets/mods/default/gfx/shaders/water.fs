#version 150

in vec3 fragmentPosition;
in vec3 fragmentNormal;
in vec2 fragmentFlow;

out vec4 fragColor;

void main()
{
    fragColor = vec4(0.2, 0.2, 0.5, 0.5);
}