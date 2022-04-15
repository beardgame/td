#version 150

uniform sampler2D normalBuffer;
uniform sampler2D depthBuffer;

uniform vec2 resolution;

uniform vec3 farPlaneBaseCorner;
uniform vec3 farPlaneUnitX;
uniform vec3 farPlaneUnitY;
uniform vec3 cameraPosition;

in vec2 lightCenterUV;
in vec2 fragmentXY;
in vec3 lightPosition;
in vec3 lightDirection;
in float lightAngleCos;
in float lightRadiusSquared;
in vec4 lightColor;

out vec4 outRGB;

float diffuseLightAmount(vec3 surfaceNormal, vec3 vectorToLight)
{
    vec3 lightNormal = normalize(vectorToLight);

    float f = dot(lightNormal, surfaceNormal);

    if (f < 0)
        return 0;

    float angleToDirectionCos = dot(-lightNormal, lightDirection);

    if (angleToDirectionCos < lightAngleCos)
        return 0;

    float w = (angleToDirectionCos - lightAngleCos) / (1 - lightAngleCos);

    return f * w;
}

vec3 getFragmentPositionFromDepth(vec2 uv)
{
    uv = clamp(uv, 0.001, 0.999);

    float depth = texture(depthBuffer, uv).x;

    vec3 pointOnFarPlane = farPlaneBaseCorner
        + farPlaneUnitX * uv.x
        + farPlaneUnitY * uv.y;

    vec3 fragmentPositionRelativeToCamera = pointOnFarPlane * depth;
    vec3 fragmentPosition = fragmentPositionRelativeToCamera - cameraPosition;

    return fragmentPosition;
}

void main()
{
    vec2 uv = gl_FragCoord.xy / resolution;

    vec3 normal = texture(normalBuffer, uv).xyz;
    normal = normal * 2 - 1;

    vec3 fragmentPosition = getFragmentPositionFromDepth(uv);

    vec3 vectorToLight = lightPosition - fragmentPosition;
    float distanceToLightSquared = dot(vectorToLight, vectorToLight);

    float a = 1 - distanceToLightSquared / lightRadiusSquared;


    if (a < 0)
        discard;

    vec3 color = lightColor.rgb * lightColor.a * a;

    float diffuse = diffuseLightAmount(normal, vectorToLight);

    if (diffuse == 0)
        discard;

    vec3 rgb = color * diffuse;


    int samples = 5 + int(lightRadiusSquared);
    vec2 uvAccum = uv;
    vec2 uvStep = (lightCenterUV - uv) / (samples + 1);
    float zLimitAccum = fragmentPosition.z;
    float zLimitStep = vectorToLight.z / (samples + 1);
    float shadowUmbraFactor = 1;
    for (int i = 0; i < samples; i++)
    {
        uvAccum += uvStep;
        zLimitAccum += zLimitStep;

        float z = getFragmentPositionFromDepth(uvAccum).z;

        if (z > zLimitAccum)
        {
            float shadowDistance = abs(z - zLimitAccum) * 10;
            if (shadowDistance > 1)
                discard;

            shadowUmbraFactor = min(shadowUmbraFactor, 1 - shadowDistance);
        }
    }

    outRGB = vec4(rgb, 0) * shadowUmbraFactor;
}
