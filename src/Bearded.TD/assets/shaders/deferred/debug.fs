#version 150

uniform sampler2D bufferTexture;

in vec2 fragmentUV;

out vec4 outColor;

void main()
{
    outColor = texture(bufferTexture, fragmentUV);
}
