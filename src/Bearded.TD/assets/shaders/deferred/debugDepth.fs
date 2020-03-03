#version 150

uniform sampler2D bufferTexture;

in vec2 fragmentUV;

out vec4 outColor;

void main()
{
	float z = texture(bufferTexture, fragmentUV).r;

    outColor = vec4(z, z, z, 1);
}
