#version 150

uniform sampler2D inputTexture;

in vec2 fragmentUV;

out vec4 outColor;

void main()
{
    vec2 size = textureSize(inputTexture, 0);
    
    float pixelStepX = 1.5 / size.x;

    const float[] stepSizes = float[](
    -7, -6, -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5, 6, 7
    );
    
    const float[] gaussianWeights = float[](
    0.00874088, 0.0179975 , 0.03315999, 0.05467158, 0.0806592,
    0.10648569, 0.12579798, 0.13298454, 0.12579798, 0.10648569,
    0.0806592 , 0.05467158, 0.03315999, 0.0179975 , 0.00874088
    );
    
    vec4 color = vec4(0);

    vec2 offset = vec2(0.5) / size;
    
    for (int i = 0; i < 15; i++)
    {
        vec2 uv = vec2(fragmentUV.x + stepSizes[i] * pixelStepX, fragmentUV.y) + offset;
        color += texture(inputTexture, uv) * gaussianWeights[i];
    }
    
    outColor = color;
}
