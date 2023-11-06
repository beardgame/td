#version 150

uniform sampler2D diffuse;

in vec4 p_color;
in vec2 p_texcoord;

out vec4 fragColor;

// noise function from https://www.shadertoy.com/view/Msf3WH
// thank you Inigo Quilez!
vec2 hash( vec2 p )
{
    p = vec2( dot(p,vec2(127.1,311.7)), dot(p,vec2(269.5,183.3)) );
    return -1.0 + 2.0*fract(sin(p)*43758.5453123);
}

float noise( in vec2 p )
{
    const float K1 = 0.366025404; // (sqrt(3)-1)/2;
    const float K2 = 0.211324865; // (3-sqrt(3))/6;

    vec2  i = floor( p + (p.x+p.y)*K1 );
    vec2  a = p - i + (i.x+i.y)*K2;
    float m = step(a.y,a.x);
    vec2  o = vec2(m,1.0-m);
    vec2  b = a - o + K2;
    vec2  c = a - 1.0 + 2.0*K2;
    vec3  h = max( 0.5-vec3(dot(a,a), dot(b,b), dot(c,c) ), 0.0 );
    vec3  n = h*h*h*h*vec3( dot(a,hash(i+0.0)), dot(b,hash(i+o)), dot(c,hash(i+1.0)));
    return dot( n, vec3(70.0) );
}

void main()
{
    vec2 texSize = textureSize(diffuse, 0);
    
	vec4 c = texture(diffuse, p_texcoord);
    
    float n = noise(p_texcoord * texSize * 0.01f) * 0.5 + 0.5;
    
    float alphaForDissolve = c.a * p_color.a * 2;
    
    float actualAlpha = clamp(c.a * 5, 0, 1);

    c.rgb /= c.a;
    c.rgb *= 2;
    c.rgb *= p_color.rgb;

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
