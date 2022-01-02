#version 450

uniform sampler2D diffuse;
uniform sampler2D normal;
uniform float time;

in vec3 fragmentPosition;
in vec2 fragmentUV;
in vec4 fragmentColor;
in float fragmentDepth;

out vec4 outRGBA;

void main()
{
	vec2 uvX = dFdx(fragmentUV) * 1.5;
	vec2 uvY = dFdy(fragmentUV) * 1.5;

    float aX = texture(diffuse, fragmentUV + uvX).a - texture(diffuse, fragmentUV - uvX).a;
    float aY = texture(diffuse, fragmentUV + uvY).a - texture(diffuse, fragmentUV - uvY).a;

    float a = aX * aX + aY * aY;


    vec3 nX = (texture(diffuse, fragmentUV + uvX).xyz * 2 - 1)
            - (texture(diffuse, fragmentUV - uvX).xyz * 2 - 1);
    vec3 nY = (texture(diffuse, fragmentUV + uvY).xyz * 2 - 1)
            - (texture(diffuse, fragmentUV - uvY).xyz * 2 - 1);

    float n = dot(nX, nX) + dot(nY, nY);

    bool isOutline = n > 0.01 || a > 0.01;

    if (isOutline)
    {
        float flicker = 0.5 + 0.3 * sin(fragmentPosition.y * 3 + time * 5);

        outRGBA = vec4(fragmentColor.rgb * max(a, n) * flicker, 0);
    }
    else
    {
        discard;
    }
}
