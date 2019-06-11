#version 150

in vec2 v_position;
in vec2 v_texCoord;

out vec2 fragmentUV;

void main()
{
    gl_Position = vec4(v_position, 0, 1);
    fragmentUV = v_texCoord;
}
