#version 410

layout (vertices = 3) out;

uniform sampler2D heightmap;
uniform float heightmapRadius;

in vec2 vertexPositionTCS[];
in vec4 vertexColorTCS[];

out vec2 vertexPositionTES[];
out vec4 vertexColorTES[];

vec2 getHeightmapUV(vec2 position)
{
    return position
    / heightmapRadius // -1..1
    * 0.5 + 0.5; // 0..1
}

void main()
{
    vertexPositionTES[gl_InvocationID] = vertexPositionTCS[gl_InvocationID];
    vertexColorTES[gl_InvocationID] = vertexColorTCS[gl_InvocationID];

    vec2 uv0 = getHeightmapUV(vertexPositionTCS[0]);
    vec2 uv1 = getHeightmapUV(vertexPositionTCS[1]);
    vec2 uv2 = getHeightmapUV(vertexPositionTCS[2]);

    float h0 = texture(heightmap, uv0).x;
    float h1 = texture(heightmap, uv1).x;
    float h2 = texture(heightmap, uv2).x;

    const float resolution = 6;
    const float maxHeightD = 1;

    float t0 = 1 + min(maxHeightD, abs(h1 - h2)) * resolution;
    float t1 = 1 + min(maxHeightD, abs(h2 - h0)) * resolution;
    float t2 = 1 + min(maxHeightD, abs(h0 - h1)) * resolution;

    float ti = max(t0, max(t1, t2)) * 2 - 1;

    gl_TessLevelOuter[0] = t0;
    gl_TessLevelOuter[1] = t1;
    gl_TessLevelOuter[2] = t2;
    gl_TessLevelInner[0] = ti;
}
