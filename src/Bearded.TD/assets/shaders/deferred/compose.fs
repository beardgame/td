#version 150

uniform sampler2D albedoTexture;
uniform sampler2D lightTexture;
uniform sampler2D depthBuffer;

uniform vec3 farPlaneBaseCorner;
uniform vec3 farPlaneUnitX;
uniform vec3 farPlaneUnitY;
uniform vec3 cameraPosition;

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

void main()
{

    vec4 albedo = texture(albedoTexture, fragmentUV);
    vec3 lightTexture = texture(lightTexture, fragmentUV).rgb;

    vec3 fragmentPosition = getFragmentPositionFromDepth(fragmentUV);
    float ambientFalloff = fragmentPosition.z > 0 ? 1 : 0.2;
    float floorAmbient = max(1 - abs(fragmentPosition.z) * ambientFalloff, 0);

    vec3 light = lightTexture + vec3(0.1) * floorAmbient;

    vec3 rgb = albedo.rgb * light;

    outColor = vec4(rgb, albedo.a);
}
