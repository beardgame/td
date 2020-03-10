#version 150

uniform sampler2D inputTexture;
uniform vec2 uvOffset;

in vec2 fragmentUV;

void main()
{
    gl_FragDepth = texture(inputTexture, fragmentUV + uvOffset).r;
}
