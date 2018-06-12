#version 150

uniform sampler2D normalBuffer;
uniform sampler2D depthBuffer;

// inject all of these
uniform vec3 cameraPosition;

in vec2 fragmentUV;
in vec2 fragmentXY;
in vec3 lightPosition;
in float lightRadiusSquared;
in vec3 lightColor;

out vec4 outRGB;

void main()
{
    vec3 normal = texture(normalBuffer, fragmentUV).xyz;
    normal = normal * 2 - 1;
    float fragmentZ = texture(depthBuffer, fragmentUV).x;

    // float depth = texture(depthBuffer, fragmentUV).x; // 0-1 in frustrum

    // vec3 fragmentPositionRelativeToCamera = pointOnFarPlane * depth;
    // vec3 fragmentPosition = fragmentPositionRelativeToCamera + cameraPosition;

    vec3 fragmentPosition = vec3(fragmentXY, fragmentZ);

    vec3 vectorToLight = lightPosition - fragmentPosition;
    float distanceToLightSquared = dot(vectorToLight, vectorToLight);

    float a = 1 - distanceToLightSquared / lightRadiusSquared;

    if (a < 0)
        discard;

    vec3 lightNormal = normalize(vectorToLight);

    float f = dot(lightNormal, normal);

    if (f < 0)
        discard;

    vec3 rgb = lightColor * (a * f);

    outRGB = vec4(rgb, 0);
}
