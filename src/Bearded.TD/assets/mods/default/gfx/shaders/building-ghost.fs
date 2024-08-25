#version 150

uniform sampler2D diffuse;

in vec3 fragmentPosition;
in vec2 fragmentUV;
in vec4 fragmentColor;
in float fragmentDepth;

out vec4 outRGBA;

void main()
{
	vec2 uvX = dFdx(fragmentUV) * 1.5;
	vec2 uvY = dFdy(fragmentUV) * 1.5;
	
	vec4 color = texture(diffuse, fragmentUV);

    float aX = texture(diffuse, fragmentUV + uvX).a - texture(diffuse, fragmentUV - uvX).a;
    float aY = texture(diffuse, fragmentUV + uvY).a - texture(diffuse, fragmentUV - uvY).a;

    float a = aX * aX + aY * aY;


    vec3 nX = (texture(diffuse, fragmentUV + uvX).xyz * 2 - 1)
            - (texture(diffuse, fragmentUV - uvX).xyz * 2 - 1);
    vec3 nY = (texture(diffuse, fragmentUV + uvY).xyz * 2 - 1)
            - (texture(diffuse, fragmentUV - uvY).xyz * 2 - 1);

    float n = dot(nX, nX) + dot(nY, nY);
    
    float insideAlpha = 0.75;
    vec4 edgeColor = mix(vec4(0.5), vec4(fragmentColor.rgb, 0), 0.25);
    vec4 insideColor = mix(vec4(0.5, 0.5, 0.5, 1), color, 0.25);
    
    float outline = max(a, n);
    float inside = color.a * insideAlpha;
    float alpha = max(outline, inside);
    
    outRGBA = mix(insideColor, edgeColor, outline) * alpha;
}
