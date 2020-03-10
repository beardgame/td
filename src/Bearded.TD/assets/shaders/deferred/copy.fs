#version 150

uniform sampler2D inputTexture;
uniform vec2 uvOffset;

in vec2 fragmentUV;

out vec4 outColor;

void main()
{
    outColor = texture(inputTexture, fragmentUV + uvOffset);
}
