#version 150

uniform sampler2D inputTexture;

in vec2 fragmentUV;

void main()
{
    gl_FragDepth = texture(inputTexture, fragmentUV).r;
}
