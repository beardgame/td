#version 150

in vec3 fragmentBiomeId;
in vec3 baricentricCoords;

out vec4 fragColor;

void main()
{
    vec3 ids = fragmentBiomeId;
    vec3 coords = baricentricCoords;

    float outId0;
    float outId1;
    float outInterpolate;
    
    if (coords.x > coords.y)
    {
        // keep x
        outId0 = ids.x;
        if (coords.z > coords.y)
        {
            // keep xz
            outId1 = ids.z;
            outInterpolate = coords.x / (coords.x + coords.z);
        }
        else
        {
            // keep xy
            outId1 = ids.y;
            outInterpolate = coords.x / (coords.x + coords.y);
        }
    }
    else
    {
        // keep y
        outId0 = ids.y;
        if (coords.z > coords.x)
        {
            // keep yz
            outId1 = ids.z;
            outInterpolate = coords.y / (coords.y + coords.z);
        }
        else
        {
            // keep yx
            outId1 = ids.x;
            outInterpolate = coords.y / (coords.y + coords.x);
        }
    }
    
    if (outId0 == outId1)
        outInterpolate = 0;
    
    fragColor = vec4(outId0 / 255, outId1 / 255, outInterpolate, 1);
}
