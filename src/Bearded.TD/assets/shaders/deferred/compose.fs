#version 150

uniform sampler2D albedoTexture;
uniform sampler2D lightTexture;
uniform sampler2D depthBuffer;

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

vec3 aces_approx(vec3 v)
{
    v *= 0.6;
    float a = 2.51;
    float b = 0.03;
    float c = 2.43;
    float d = 0.59;
    float e = 0.14;
    return clamp((v*(a*v+b))/(v*(c*v+d)+e), 0.0, 1.0);
}

void main()
{
    vec4 albedo = texture(albedoTexture, fragmentUV);
    vec3 lightTexture = texture(lightTexture, fragmentUV).rgb;

    vec3 fragmentPosition = getFragmentPositionFromDepth(fragmentUV);
    float ambientFalloff = fragmentPosition.z > 0 ? 0 : 0.2;
    float floorAmbient = max(1 - abs(fragmentPosition.z) * ambientFalloff, 0);

    vec3 rgb = lightTexture + albedo.rgb * floorAmbient * 0;

    float hexagonalDistanceToOrigin = hexDistanceToOrigin(fragmentPosition.xy);
    float falloff = clamp((hexagonalFallOffDistance - hexagonalDistanceToOrigin) * 0.3f, 0, 1);
    falloff *= falloff;

    rgb *= falloff;
    
    vec2 heightMapUV =
        fragmentPosition.xy / heightmapRadius // -1..1
        * 0.5 + 0.5; // 0..1

    vec4 heightMapValue = texture(heightmap, heightMapUV);
    float visibility = heightMapValue.g;

    rgb *= visibility;
    
    rgb = aces_approx(rgb);

    outColor = vec4(rgb, albedo.a);
}
