#version 150

uniform mat4 projection;
uniform mat4 view;

in vec3 v_position;
in vec2 v_texcoord;
in vec4 v_color;

out vec2 fragmentUV;
out vec4 fragmentColor;
out float fragmentDepth;

void main()
{
    gl_Position = projection * view * vec4(v_position, 1.0);
    fragmentUV = v_texcoord;
    fragmentColor = v_color;
    fragmentDepth = v_position.z;
}
