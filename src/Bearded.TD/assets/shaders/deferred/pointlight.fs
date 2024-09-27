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
in float lightShadow;

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

float getSelfShadowFactor(vec2 uv, vec3 fragmentPosition, vec3 vectorToLight, float directLighting)
{
    const float shadowLength = 30;
    const int samples = 15;
    const float hardness = 5;
    
    float distanceToLight = length(vectorToLight);
    float maxDistanceToLight = sqrt(lightRadiusSquared);
    float normalisedDistanceToLight = distanceToLight / maxDistanceToLight;
    
    vec2 uvRay = normalize(lightCenterUV - uv) / resolution * shadowLength * normalisedDistanceToLight;
    float step = 1.0 / samples;
    float rayT = 0;

    float slope = directLighting;
    float maxSlope = 0;
    float shadow = 0;

    vec3 lightNormal = normalize(lightPosition - fragmentPosition);
    
    for (int i = 0; i < samples; i++)
    {
        vec2 sampleUV = uv + uvRay * rayT;
        vec3 normal = texture(normalBuffer, sampleUV).xyz;
        normal = normal * 2 - 1;
        
        float f = dot(lightNormal, normal);
        
        slope += f;
        if (slope < maxSlope)
        {
            shadow += 1 - rayT;
        }
        maxSlope = max(maxSlope, slope);

        rayT += step;
    }
    return clamp(1 - hardness * shadow / samples, 0, 1);
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

    vec3 rgb = lightColor.rgb * lightColor.a * (a * f) * 5;
    
    bool enableShadow = lightShadow > 0;
    
    float shadowUmbraFactor = enableShadow
        ? getShadowUmbraFactor(uv, fragmentPosition, vectorToLight)
        : 1;
    
    float selfShadow = getSelfShadowFactor(uv, fragmentPosition, vectorToLight, f);

    outRGB = vec4(rgb, 0) * shadowUmbraFactor * selfShadow;


    //outRGB = vec4(lightCenterUV, 0, 1);
    //outRGB = vec4(uv - lightCenterUV, 0, 0);
}
