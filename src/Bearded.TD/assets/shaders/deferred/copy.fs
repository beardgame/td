#version 150

uniform sampler2D inputTexture;

in vec2 fragmentUV;

out vec4 outColor;

void main()
{
    outColor = texture(inputTexture, fragmentUV);
}
