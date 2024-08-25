#version 150

uniform sampler2D albedoTexture;
uniform sampler2D lightTexture;
uniform sampler2D normalBuffer;
uniform sampler2D depthBuffer;

uniform vec3 farPlaneBaseCorner;
uniform vec3 farPlaneUnitX;
uniform vec3 farPlaneUnitY;
uniform vec3 cameraPosition;

uniform vec2 resolution;

uniform int cascadeIndex;

const vec2[] cascade0directions = vec2[](
    vec2(-1, -1),
    vec2(1, -1),
    vec2(-1, 1),
    vec2(1, 1)
);

in vec2 fragmentUV;

out vec3 outColor;

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
    ivec2 outTexel = ivec2(fragmentUV * 512);
    int index = (outTexel.x % 2) + (outTexel.y % 2) * 2;
    vec2 direction = cascade0directions[index];
    
    vec2 step = direction / resolution;
    
    const int count = 20;
    
    vec4 light = vec4(0);
    
    vec3 fragmentP = getFragmentPositionFromDepth(fragmentUV);
    vec3 fragmentN = texture(normalBuffer, fragmentUV).xyz * 2 - 1;
    
    for (int i = 0; i < count; i++)
    {
        vec2 uv = fragmentUV + step * float(i) * 3;

        vec3 p = getFragmentPositionFromDepth(uv);
        vec3 n = texture(normalBuffer, uv).xyz * 2 - 1;
        vec3 d = fragmentP - p;
        vec3 dn = normalize(d);
        float contribution = max(0, dot(n, dn));
        float c = max(0, dot(fragmentN, -dn));
        
        light += texture(lightTexture, uv) * contribution * c;
    }
    
    //light /= count;

    outColor = vec3(light.rgb) * 1;
}
