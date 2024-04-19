#version 150

uniform sampler2D inputTexture;

in vec2 fragmentUV;

out vec4 outColor;

void main()
{
    vec2 size = textureSize(inputTexture, 0);
    
    float pixelStepX = 1.0 / size.x;
    
    const float[] gaussianWeights = float[](
        0.005977, 0.060598, 0.24173, 0.382925, 0.24173, 0.060598, 0.005977
    );
    
    vec4 color = vec4(0);
    
    for (int i = 0; i < 7; i++)
    {
        vec2 uv = vec2(fragmentUV.x + float(i - 3) * pixelStepX, fragmentUV.y);
        color += texture(inputTexture, uv) * gaussianWeights[i];
    }
    
    outColor = color;
}
