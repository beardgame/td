#version 150

uniform mat4 projection;
uniform mat4 view;

in vec3 vertexPosition;

in vec3 instanceLightPosition;
in float instanceLightRadius;
in vec4 instanceLightColor;
in float instanceIntensity;
in float instanceShadow;

out vec2 lightCenterUV;
out vec3 lightPosition;
out float lightRadiusSquared;
out vec4 lightColor;
out float lightShadow;

void main()
{
    vec4 p = projection * view * vec4(instanceLightPosition + vertexPosition * instanceLightRadius, 1.0);
    gl_Position = p;

    vec4 lightPositionTransformed = projection * view * vec4(instanceLightPosition, 1.0);

    lightCenterUV = (lightPositionTransformed.xy / lightPositionTransformed.w)
    	* 0.5 + 0.5;

    lightPosition = instanceLightPosition;
    lightRadiusSquared = instanceLightRadius * instanceLightRadius;
    lightColor = instanceLightColor * instanceIntensity;
    lightShadow = instanceShadow;
}
