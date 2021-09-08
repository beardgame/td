#version 150

uniform mat4 projection;
uniform mat4 view;

in vec3 vertexPosition;
in vec3 vertexLightPosition;
in vec3 vertexLightDirection;
in float vertexLightAngle;
in float vertexLightRadiusSquared;
in vec4 vertexLightColor;

out vec2 lightCenterUV;
out vec2 fragmentXY;
out vec3 lightPosition;
out vec3 lightDirection;
out float lightAngleCos;
out float lightRadiusSquared;
out vec4 lightColor;

void main()
{

    vec4 p = projection * view * vec4(vertexPosition, 1.0);
    gl_Position = p;


    vec4 lightPositionTransformed = projection * view * vec4(vertexLightPosition, 1.0);

    lightCenterUV = (lightPositionTransformed.xy / lightPositionTransformed.w)
    	* 0.5 + 0.5;


    fragmentXY = vertexPosition.xy;
    lightPosition = vertexLightPosition;
    lightDirection = vertexLightDirection;
    lightAngleCos = cos(vertexLightAngle);
    lightRadiusSquared = vertexLightRadiusSquared;
    lightColor = vertexLightColor;
}
