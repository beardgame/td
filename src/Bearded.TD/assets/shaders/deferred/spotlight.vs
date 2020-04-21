#version 150

uniform mat4 projection;
uniform mat4 view;

uniform vec3 farPlaneBaseCorner;
uniform vec3 farPlaneUnitX;
uniform vec3 farPlaneUnitY;

in vec3 vertexPosition;
in vec3 vertexLightPosition;
in vec3 vertexLightDirection;
in float vertexLightAngle;
in float vertexLightRadiusSquared;
in vec4 vertexLightColor;

out vec2 fragmentUV;
out vec2 fragmentXY;
out vec3 lightPosition;
out vec3 lightDirection;
out float lightAngleCos;
out float lightRadiusSquared;
out vec4 lightColor;
out vec3 pointOnFarPlane;

void main()
{

    vec4 p = projection * view * vec4(vertexPosition, 1.0);
    gl_Position = p;

    fragmentUV = (p.xy / p.w + 1) * 0.5;

    // interpolation along the diagonal might be broken?
    // probably only for weird geometry
    pointOnFarPlane = farPlaneBaseCorner
        + farPlaneUnitX * fragmentUV.x
        + farPlaneUnitY * fragmentUV.y;

    fragmentXY = vertexPosition.xy;
    lightPosition = vertexLightPosition;
    lightDirection = vertexLightDirection;
    lightAngleCos = cos(vertexLightAngle);
    lightRadiusSquared = vertexLightRadiusSquared;
    lightColor = vertexLightColor;
}
