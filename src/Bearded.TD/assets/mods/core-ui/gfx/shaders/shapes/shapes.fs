#version 150

const int SHAPE_TYPE_FILL = 0;
const int SHAPE_TYPE_LINE = 1; // point to point
const int SHAPE_TYPE_CIRCLE = 2; // center and radius

const int EDGE_OUTER_WIDTH_I = 0;
const int EDGE_INNER_WIDTH_I = 1;
const int EDGE_OUTER_GLOW_I = 2;
const int EDGE_INNER_GLOW_I = 3;

const int COLOR_FILL_I = 0;
const int COLOR_EDGE_I = 1;
const int COLOR_GLOW_OUTER_I = 2;
const int COLOR_GLOW_INNER_I = 3;

const float ANTI_ALIAS_WIDTH = 1;

in vec3 p_position;
flat in int p_shapeType;
flat in vec4 p_shapeData;
flat in vec4 p_edgeData;
flat in ivec4 p_shapeColors;

out vec4 fragColor;

float signedDistanceToEdge()
{
    vec2 p0 = p_position.xy;
    switch(p_shapeType) {
        case SHAPE_TYPE_LINE:
            vec2 p1 = p_shapeData.xy;
            vec2 p2 = p_shapeData.zw;
            vec2 v12 = p2 - p1;
            vec2 v01 = p1 - p0;
            float numerator = v12.x * v01.y - v12.y * v01.x;
            float denominator = length(v12);
            return numerator / denominator;
        case SHAPE_TYPE_CIRCLE:
            vec2 center = p_shapeData.xy;
            float radius = p_shapeData.z;
            return length(p0 - center) - radius;
    }
    
    return 0;
}

vec4 rgbaIntToVec4(int rgba)
{
    return vec4(
    ((rgba >> 0) & 0xFF) / 255.0,
    ((rgba >> 8) & 0xFF) / 255.0,
    ((rgba >> 16) & 0xFF) / 255.0,
    ((rgba >> 24) & 0xFF) / 255.0
    );
}

vec4 getColor(int index)
{
    return rgbaIntToVec4(p_shapeColors[index]);
}

struct Contribution
{
    float alpha;
};

Contribution contributionOf(float minDistance, float maxDistance, float distance, float antiAliasWidth)
{
    float edgeRadius = (maxDistance + minDistance + antiAliasWidth) / 2;
    float edgeCenterOffset = (maxDistance - minDistance) / 2;
    float absoluteDistance = abs(distance - edgeCenterOffset);

    float alpha = clamp((edgeRadius - absoluteDistance) / antiAliasWidth, 0, 1);
    
    return Contribution(alpha);
}

vec4 edgeContribution(float distance, float antiAliasWidth)
{
    float edgeOuterWidth = p_edgeData[EDGE_OUTER_WIDTH_I];
    float edgeInnerWidth = p_edgeData[EDGE_INNER_WIDTH_I];
    
    Contribution c = contributionOf(edgeInnerWidth, edgeOuterWidth, distance, antiAliasWidth);
    
    return getColor(COLOR_EDGE_I) * c.alpha;
}

vec4 fillContribution(float distance, float antiAliasWidth)
{
    float alpha = clamp((0 - distance) / antiAliasWidth, 0, 1);
    return getColor(COLOR_FILL_I) * alpha;
}


const bool DEBUG_EDGES = false;

void main()
{
    fragColor = vec4(0);
    
    if (p_shapeType == SHAPE_TYPE_FILL)
    {
        fragColor = getColor(COLOR_FILL_I);
        if (DEBUG_EDGES)
        {
            fragColor = mix(fragColor, vec4(0, 0, 1, 1), 0.3);
        }
        return;
    }

    float signedDistance = signedDistanceToEdge();
    float antiAliasWidth = ANTI_ALIAS_WIDTH * length(vec2(dFdx(signedDistance), dFdy(signedDistance)));
    
    vec4 fill = fillContribution(signedDistance, antiAliasWidth);
    vec4 edges = edgeContribution(signedDistance, antiAliasWidth);

    fragColor += mix(fill, edges, edges.a);

    if (DEBUG_EDGES)
    {
        float a = 0.8 + cos(signedDistance * 3.14) * 0.2;
        if (p_shapeType == SHAPE_TYPE_LINE)
        {
            fragColor = mix(fragColor, vec4(1, 0, 0, 1) * a, 0.3);
        }
        if (p_shapeType == SHAPE_TYPE_CIRCLE)
        {
            fragColor = mix(fragColor, vec4(0, 1, 0, 1) * a, 0.3);
        }
    }
}
