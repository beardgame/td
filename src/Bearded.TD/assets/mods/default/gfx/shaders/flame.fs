#version 150

uniform sampler2D diffuse;

in vec4 p_color;
in vec2 p_texcoord;

out vec4 fragColor;

void main()
{
    vec2 texSize = textureSize(diffuse, 0);

	vec4 c = texture(diffuse, p_texcoord);
    
    float alphaForDissolve = c.a * p_color.a * 2;
    
    float actualAlpha = clamp(c.a * 5, 0, 1);

    c.rgb /= c.a * 1.5;
    c.rgb *= 2;
    c.rgb *= p_color.rgb;

    const float n = 0.3;
    const float edge = 0.1;
    float s = smoothstep(n - edge, n + edge, alphaForDissolve * actualAlpha);
    s = clamp(s, 0, 1);
    
    c *= s;
    
    if (s < 0.001)
    {
        discard;
    }
    
    fragColor = c;
}
