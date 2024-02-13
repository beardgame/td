#version 150

const int SHAPE_TYPE_FILL = 0;
const int SHAPE_TYPE_LINE = 1; // point to point
const int SHAPE_TYPE_CIRCLE = 2; // center and radius

const int EDGE_OUTER_WIDTH_I = 0;
const int EDGE_INNER_WIDTH_I = 1;
const int EDGE_OUTER_GLOW_I = 2;
const int EDGE_INNER_GLOW_I = 3;

const float ANTI_ALIAS_WIDTH = 1;

// TODO: deprecate and replace with gradient definition
in vec4 p_color;

in vec3 p_position;
flat in int p_shapeType;
flat in vec4 p_shapeData;
flat in vec4 p_edgeData;

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

vec4 edgeContribution(float distance, float antiAliasWidth)
{
    float edgeOuterWidth = p_edgeData[EDGE_OUTER_WIDTH_I];
    float edgeInnerWidth = p_edgeData[EDGE_INNER_WIDTH_I];

    float edgeRadius = (edgeOuterWidth + edgeInnerWidth + antiAliasWidth) / 2;
    float edgeCenterOffset = (edgeOuterWidth - edgeInnerWidth) / 2;
    float absoluteDistance = abs(distance - edgeCenterOffset);

    float alpha = clamp((edgeRadius - absoluteDistance) / antiAliasWidth, 0, 1);
    
    return p_color * alpha;
}

const bool DEBUG_EDGES = false;

void main()
{
    fragColor = vec4(0);
    
    if (p_shapeType == SHAPE_TYPE_FILL)
    {
        //fragColor = p_color;
        if (DEBUG_EDGES)
        {
            fragColor = mix(fragColor, vec4(0, 0, 1, 1), 0.3);
        }
        return;
    }

    float signedDistance = signedDistanceToEdge();
    
    float pixelDistance = length(vec2(dFdx(signedDistance), dFdy(signedDistance)));
    float antiAliasWidth = ANTI_ALIAS_WIDTH * pixelDistance;
    
    vec4 edge = edgeContribution(signedDistance, antiAliasWidth);
    
    fragColor += edge;

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
