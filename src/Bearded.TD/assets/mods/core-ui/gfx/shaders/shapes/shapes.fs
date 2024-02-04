#version 150

in vec4 p_color;

out vec4 fragColor;

void main()
{
    vec4 position = gl_FragCoord;
    fragColor = p_color;
}
