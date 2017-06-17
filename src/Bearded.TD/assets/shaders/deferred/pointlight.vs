#version 150

uniform mat4 projection;
uniform mat4 view;

in vec3 vertexPosition;
in vec3 vertexLightPosition;
in float vertexLightRadiusSquared;
in vec3 vertexLightColor;

out vec2 fragmentUV;
out vec2 fragmentXY;
out vec3 lightPosition;
out float lightRadiusSquared;
out vec3 lightColor;

void main()
{
    vec4 p = projection * view * vec4(vertexPosition, 1.0);
    gl_Position = p;

    fragmentUV = (p.xy / p.w + 1) * 0.5;

    fragmentXY = vertexPosition.xy;
    lightPosition = vertexLightPosition;
    lightRadiusSquared = vertexLightRadiusSquared;
    lightColor = vertexLightColor;
}
