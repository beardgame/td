﻿#version 150

in float f_height;

out vec4 fragColor;

void main()
{
    fragColor = vec4(f_height, f_height, f_height, 1);
}
