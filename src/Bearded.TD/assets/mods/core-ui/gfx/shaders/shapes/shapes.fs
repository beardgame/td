#version 150

const int SHAPE_TYPE_FILL = 0;
const int SHAPE_TYPE_LINE = 1; // point to point
const int SHAPE_TYPE_CIRCLE = 2; // center and radius

const int EDGE_OUTER_WIDTH_I = 0;
const int EDGE_INNER_WIDTH_I = 1;
const int EDGE_OUTER_GLOW_I = 2;
const int EDGE_INNER_GLOW_I = 3;

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

#define DEBUG_GEOMETRY false

void main()
{
    if (p_shapeType == SHAPE_TYPE_FILL)
    {
        //fragColor = p_color;
        return;
    }
    #if DEBUG_GEOMETRY
    if (p_shapeType == SHAPE_TYPE_LINE)
    {
        fragColor = vec4(1, 0, 0, 1);
        return;
    }
    if (p_shapeType == SHAPE_TYPE_CIRCLE)
    {
        fragColor = vec4(0, 1, 0, 1);
        return;
    }
    #endif
    
    float edgeOuterWidth = p_edgeData[EDGE_OUTER_WIDTH_I];
    float edgeInnerWidth = p_edgeData[EDGE_INNER_WIDTH_I];
    float edgeOuterGlow = p_edgeData[EDGE_OUTER_GLOW_I];
    float edgeInnerGlow = p_edgeData[EDGE_INNER_GLOW_I];
    
    float edgeDistance = signedDistanceToEdge() + 1.5;

    float alpha = 1 - edgeDistance / length(vec2(dFdx(edgeDistance), dFdy(edgeDistance)));
    
    
    alpha = 1 - abs(alpha);
    
    if(alpha < 0)
    {
        discard;
    }
    
    fragColor = p_color * alpha;
}
