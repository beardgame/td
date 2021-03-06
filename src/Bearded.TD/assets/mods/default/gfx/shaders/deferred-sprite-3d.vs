﻿#version 150

uniform mat4 projection;
uniform mat4 viewLevel;

uniform float farPlaneDistance;

in vec3 vertexPosition;
in vec3 vertexNormal;
in vec3 vertexTangent;
in vec2 vertexUV;
in vec4 vertexColor;

out vec3 fragmentNormal;
out vec3 fragmentTangent;
out vec2 fragmentUV;
out vec4 fragmentColor;
out float fragmentDepth;


void main()
{
	vec4 viewPosition = viewLevel * vec4(vertexPosition, 1.0);
	vec4 position = projection * viewPosition;
    gl_Position = position;

    fragmentNormal = vertexNormal;
    fragmentTangent = vertexTangent;
	fragmentColor = vertexColor;
	fragmentUV = vertexUV;
	fragmentDepth = -viewPosition.z / farPlaneDistance;
}