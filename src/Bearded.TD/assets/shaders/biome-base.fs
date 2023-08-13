#version 150

flat in uint fragmentBiomeId;

out vec4 fragColor;

void main()
{
    float value = fragmentBiomeId / 255.0;
    
    fragColor = vec4(value, value, value, 1);
}
