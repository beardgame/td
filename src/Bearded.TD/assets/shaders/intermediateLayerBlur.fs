#version 150

uniform sampler2D inputTexture;

in vec2 fragmentUV;

out vec4 outColor;

void main()
{
    vec2 size = textureSize(inputTexture, 0);
    
    float pixelStepX = 1.0 / size.x;
    
    const float[] gaussianWeights = float[](
    0.00874088, 0.0179975 , 0.03315999, 0.05467158, 0.0806592,
    0.10648569, 0.12579798, 0.13298454, 0.12579798, 0.10648569,
    0.0806592 , 0.05467158, 0.03315999, 0.0179975 , 0.00874088
    );
    
    vec4 color = vec4(0);
    
    for (int i = 0; i < 15; i++)
    {
        vec2 uv = vec2(fragmentUV.x + float(i - 7) * pixelStepX, fragmentUV.y);
        color += texture(inputTexture, uv) * gaussianWeights[i];
    }
    
    outColor = color;
}
