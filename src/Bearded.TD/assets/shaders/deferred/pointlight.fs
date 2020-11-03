#version 150

uniform sampler2D normalBuffer;
uniform sampler2D depthBuffer;

uniform vec2 resolution;

uniform vec3 farPlaneBaseCorner;
uniform vec3 farPlaneUnitX;
uniform vec3 farPlaneUnitY;

uniform vec3 cameraPosition;

in vec2 fragmentXY;
in vec3 lightPosition;
in float lightRadiusSquared;
in vec4 lightColor;
out vec4 outRGB;

void main()
{
    vec2 uv = gl_FragCoord.xy / resolution;

    vec3 pointOnFarPlane = farPlaneBaseCorner
        + farPlaneUnitX * uv.x
        + farPlaneUnitY * uv.y;

    vec3 normal = texture(normalBuffer, uv).xyz;
    normal = normal * 2 - 1;

    float depth = texture(depthBuffer, uv).x;

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

    vec3 rgb = lightColor.rgb * lightColor.a * (a * f);

    outRGB = vec4(rgb, 0);

}
