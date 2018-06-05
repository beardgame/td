#version 150

uniform mat4 projection;
uniform mat4 view;

// inject this
uniform float farPlaneDistance;

in vec3 vertexPosition;
in vec3 vertexNormal;
in vec2 vertexUV;
in vec4 vertexColor;

out vec3 fragmentNormal;
out vec2 fragmentUV;
out vec4 fragmentColor;
out float fragmentDepth;

void main()
{
	vec4 viewPosition = view * vec4(vertexPosition, 1.0);
	vec4 position = projection * viewPosition;
    gl_Position = position;
    fragmentNormal = vertexNormal;
    fragmentUV = vertexUV;
    fragmentColor = vertexColor;

    // check if this is actually in 0-1 space between camera and far plane
    float depth = -viewPosition.z / farPlaneDistance;
    fragmentDepth = depth;
}
