#version 150

uniform sampler2D normalBuffer;
uniform sampler2D depthBuffer;

uniform vec2 resolution;

uniform vec3 farPlaneBaseCorner;
uniform vec3 farPlaneUnitX;
uniform vec3 farPlaneUnitY;
uniform vec3 cameraPosition;

in vec2 lightCenterUV;
in vec3 lightPosition;
in float lightRadiusSquared;
in vec4 lightColor;
out vec4 outRGB;

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

float dither(vec2 xy)
{
    return fract(dot(xy, vec2(36, 7) / 16.0f));
}

float getShadowUmbraFactor(vec2 uv, vec3 fragmentPosition, vec3 vectorToLight)
{
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
    
    return shadowUmbraFactor;
}

void main()
{
    vec2 uv = gl_FragCoord.xy / resolution;

    vec3 fragmentPosition = getFragmentPositionFromDepth(uv);

    vec3 vectorToLight = lightPosition - fragmentPosition;
    float distanceToLightSquared = dot(vectorToLight, vectorToLight);

    float a = 1 - distanceToLightSquared / lightRadiusSquared;

    if (a < 0)
        discard;

    vec3 normal = texture(normalBuffer, uv).xyz;
    normal = normal * 2 - 1;

    vec3 lightNormal = normalize(vectorToLight);

    float f = dot(lightNormal, normal);

    if (f < 0)
        discard;

    vec3 rgb = lightColor.rgb * lightColor.a * (a * f);

    float shadowUmbraFactor = getShadowUmbraFactor(uv, fragmentPosition, vectorToLight);

    outRGB = vec4(rgb, 0) * shadowUmbraFactor;


    //outRGB = vec4(lightCenterUV, 0, 1);
    //outRGB = vec4(uv - lightCenterUV, 0, 0);
}
