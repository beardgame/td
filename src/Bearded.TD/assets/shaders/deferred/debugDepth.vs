#version 150

in vec3 v_position;
in vec2 v_texcoord;

out vec2 fragmentUV;

void main()
{
    gl_Position = vec4(v_position, 1);
    fragmentUV = v_texcoord;
}
