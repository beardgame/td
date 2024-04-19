#version 150

uniform mat4 projection;
uniform mat4 view;

in vec3 v_position;

out vec2 fragmentUV;

void main()
{
    vec4 p = projection * view * vec4(v_position, 1.0);
    fragmentUV = (p.xy / p.w) * 0.5 + 0.5;
    gl_Position = p;
}
