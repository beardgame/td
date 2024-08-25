#version 150

in vec2 v_position;

out vec2 fragmentUV;

void main()
{
    gl_Position = vec4(v_position, 0, 1);
    fragmentUV = v_position * 0.5 + 0.5;
}
