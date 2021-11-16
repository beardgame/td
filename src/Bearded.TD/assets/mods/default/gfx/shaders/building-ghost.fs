#version 450

uniform sampler2D diffuse;
uniform sampler2D normal;
uniform float time;

in vec2 fragmentUV;
in vec4 fragmentColor;
in float fragmentDepth;

out vec4 outRGBA;
out vec4 outNormal;
out vec4 outDepth;

void main()
{
	vec2 uvX = dFdx(fragmentUV) * 1.5;
	vec2 uvY = dFdy(fragmentUV) * 1.5;

    float aX = texture(diffuse, fragmentUV + uvX).a - texture(diffuse, fragmentUV - uvX).a;
    float aY = texture(diffuse, fragmentUV + uvY).a - texture(diffuse, fragmentUV - uvY).a;

    float a = aX * aX + aY * aY;


    vec3 nX = (texture(diffuse, fragmentUV + uvX).xyz * 2 - 1) - (texture(diffuse, fragmentUV - uvX).xyz * 2 - 1);
    vec3 nY = (texture(diffuse, fragmentUV + uvY).xyz * 2 - 1) - (texture(diffuse, fragmentUV - uvY).xyz * 2 - 1);

    float n = dot(nX, nX) + dot(nY, nY);

    if (n < 0.5 && a < 0.01)
        discard;


    outRGBA = vec4(fragmentColor.rgb, 0);
    outNormal = vec4(0);
    outDepth = vec4(fragmentDepth, 0, 0, 1);
}
