#version 150

const float PI = 3.14159265359;

uniform sampler2D diffuseBuffer;
uniform sampler2D normalBuffer;
uniform sampler2D materialBuffer;
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

vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

float DistributionGGX(vec3 N, vec3 H, float roughness)
{
    float a      = roughness*roughness;
    float a2     = a*a;
    float NdotH  = max(dot(N, H), 0.0);
    float NdotH2 = NdotH*NdotH;

    float num   = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;

    return num / denom;
}

float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r*r) / 8.0;

    float num   = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return num / denom;
}
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2  = GeometrySchlickGGX(NdotV, roughness);
    float ggx1  = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
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

    float attenuation = 1 - distanceToLightSquared / lightRadiusSquared;

    if (attenuation < 0)
        discard;

    attenuation = pow(attenuation, 2);
    
    vec3 radiance = lightColor.rgb * lightColor.a * attenuation * 20;
    
    vec3 albedo = texture(diffuseBuffer, uv).xyz;

    vec4 material = texture(materialBuffer, uv);
    float metallic = material.y;
    float roughness = material.z;

    vec3 surfaceNormal = texture(normalBuffer, uv).xyz;
    surfaceNormal = surfaceNormal * 2 - 1;

    vec3 V = normalize(-fragmentPosition - cameraPosition);
    vec3 L = normalize(vectorToLight);
    vec3 N = normalize(surfaceNormal);
    vec3 H = normalize(V + L);

    vec3 F0 = vec3(0.04);
    F0 = mix(F0, albedo, metallic);
    vec3 F  = fresnelSchlick(max(dot(H, V), 0.0), F0);
    float NDF = DistributionGGX(N, H, roughness);
    float G = GeometrySmith(N, V, L, roughness);
    vec3 numerator = NDF * G * F;
    float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0)  + 0.0001;
    vec3 specular = numerator / denominator;

    vec3 kS = F;
    vec3 kD = vec3(1.0) - kS;

    kD *= 1.0 - metallic;

    float NdotL = max(dot(N, L), 0.0);
    outRGB = vec4((kD * albedo / PI + specular) * radiance * NdotL, 0);

    if (true)
    {
        float selfShadow = getSelfShadowFactor(uv, fragmentPosition, vectorToLight, NdotL);
        outRGB.rgb = outRGB.rgb * selfShadow;
    }
}
