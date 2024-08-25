#version 150

uniform sampler2D albedoTexture;
uniform sampler2D lightTexture;
uniform sampler2D normalBuffer;
uniform sampler2D depthBuffer;

uniform sampler2D radianceCascadeBuffer;

uniform vec3 farPlaneBaseCorner;
uniform vec3 farPlaneUnitX;
uniform vec3 farPlaneUnitY;
uniform vec3 cameraPosition;

uniform float hexagonalFallOffDistance;

uniform sampler2D heightmap;
uniform float heightmapRadius;
uniform float heightmapPixelSizeUV;

in vec2 fragmentUV;

out vec4 outColor;

vec3 getFragmentPositionFromDepth(vec2 uv)
{
    float depth = texture(depthBuffer, uv).x;

    vec3 pointOnFarPlane = farPlaneBaseCorner
        + farPlaneUnitX * uv.x
        + farPlaneUnitY * uv.y;

    vec3 fragmentPositionRelativeToCamera = pointOnFarPlane * depth;
    vec3 fragmentPosition = fragmentPositionRelativeToCamera - cameraPosition;

    return fragmentPosition;
}

float hexDistanceToOrigin(vec2 xy)
{
    float yf = xy.y / (1.5 / 1.73205080757);
    float xf = xy.x - yf * 0.5;
    float x = abs(xf);
    float y = abs(yf);
    float reduction = xf * yf < 0 ? min(x, y) : 0;
    return x + y - reduction;
}

const vec2[] cascade0directions = vec2[](
    vec2(-1, -1),
    vec2(1, -1),
    vec2(-1, 1),
    vec2(1, 1)
);

vec3 getRadianceFromProbe(vec3 normal, ivec2 probeTexel)
{
    vec3 radiance;

    for (int i = 0; i < 4; i++)
    {
        vec3 d = normalize(vec3(cascade0directions[i], 1));

        float contribution = max(0, dot(normal, d));

        ivec2 offset = ivec2(i % 2, i / 2);
        radiance += texelFetch(radianceCascadeBuffer, probeTexel + offset, 0).rgb * contribution;
    }

    return radiance;
}

vec3 getRadiance()
{
    vec3 normal = texture(normalBuffer, fragmentUV).xyz * 2 - 1;

    ivec2 probeTexel = ivec2(fragmentUV * 256) * 2;

    vec2 probeUV = fragmentUV * 256;
    vec2 probeInterpolation = fract(probeUV);
    
    vec3 radiance = mix(
        mix(
            getRadianceFromProbe(normal, probeTexel),
            getRadianceFromProbe(normal, probeTexel + ivec2(2, 0)),
            probeInterpolation.x
        ),
        mix(
            getRadianceFromProbe(normal, probeTexel + ivec2(0, 2)),
            getRadianceFromProbe(normal, probeTexel + ivec2(2, 2)),
            probeInterpolation.x
        ),
        probeInterpolation.y
    );
    
    return radiance;
}

void main()
{
    vec4 albedo = texture(albedoTexture, fragmentUV);
    vec3 lightTexture = texture(lightTexture, fragmentUV).rgb;

    vec3 fragmentPosition = getFragmentPositionFromDepth(fragmentUV);
    float ambientFalloff = fragmentPosition.z > 0 ? 0 : 0.2;
    float floorAmbient = max(1 - abs(fragmentPosition.z) * ambientFalloff, 0);

    vec3 light = lightTexture + vec3(0.33) * floorAmbient;

    vec3 rgb = albedo.rgb * light;

    float hexagonalDistanceToOrigin = hexDistanceToOrigin(fragmentPosition.xy);
    float falloff = clamp((hexagonalFallOffDistance - hexagonalDistanceToOrigin) * 0.3f, 0, 1);
    falloff *= falloff;

    rgb *= falloff;
    
    vec2 heightMapUV =
        fragmentPosition.xy / heightmapRadius // -1..1
        * 0.5 + 0.5; // 0..1

    vec4 heightMapValue = texture(heightmap, heightMapUV);
    float visibility = heightMapValue.g;


    vec3 radiance = getRadiance();
    
    rgb += radiance;

    rgb *= visibility;

    outColor = vec4(rgb, albedo.a);
    
    //return;

    outColor = vec4(lightTexture, 1);
    
    float r = length(radiance);
    float l = length(lightTexture);
    
    outColor = vec4(r, l, 0, 1);
    //outColor = vec4(radiance, 1);
    //outColor = vec4(lightTexture, 1);
}
