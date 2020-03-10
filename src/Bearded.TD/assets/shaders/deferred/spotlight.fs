#version 150

uniform sampler2D normalBuffer;
uniform sampler2D depthBuffer;

uniform vec3 cameraPosition;

in vec2 fragmentUV;
in vec2 fragmentXY;
in vec3 lightPosition;
in vec3 lightDirection;
in float lightAngleCos;
in float lightRadiusSquared;
in vec3 lightColor;
in vec3 pointOnFarPlane;

out vec4 outRGB;

void main()
{
    vec3 normal = texture(normalBuffer, fragmentUV).xyz;
    normal = normal * 2 - 1;

    float depth = texture(depthBuffer, fragmentUV).x;

    vec3 fragmentPositionRelativeToCamera = pointOnFarPlane * depth;
    vec3 fragmentPosition = fragmentPositionRelativeToCamera - cameraPosition;

    vec3 vectorToLight = lightPosition - fragmentPosition;
    float distanceToLightSquared = dot(vectorToLight, vectorToLight);

    float a = 1 - distanceToLightSquared / lightRadiusSquared;

    if (a < 0)
        discard;

    vec3 lightNormal = normalize(vectorToLight);

    float f = dot(lightNormal, normal);

    if (f < 0)
        discard;

    float angleToDirectionCos = dot(-lightNormal, lightDirection);

    if (angleToDirectionCos < lightAngleCos)
        discard;

    float w = (angleToDirectionCos - lightAngleCos) / (1 - lightAngleCos);

    vec3 rgb = lightColor * (a * f * w);

    outRGB = vec4(rgb, 0);
}
