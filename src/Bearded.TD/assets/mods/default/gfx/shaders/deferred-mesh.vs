#version 150

uniform mat4 projection;
uniform mat4 view;

uniform float farPlaneDistance;

in vec3 vertexPosition;
in vec3 vertexNormal;
in vec2 vertexUV;

in vec4 instanceMatrixRow1;
in vec4 instanceMatrixRow2;
in vec4 instanceMatrixRow3;
in vec4 instanceMatrixRow4;

out vec3 fragmentPosition;
out vec3 fragmentNormal;
out vec2 fragmentUV;
out float fragmentDepth;

void main()
{
    mat4 instanceMatrix = mat4(
        instanceMatrixRow1,
        instanceMatrixRow2,
        instanceMatrixRow3,
        instanceMatrixRow4);
    
    vec4 instancePosition = instanceMatrix * vec4(vertexPosition, 1.0);

    vec4 viewPosition = view * instancePosition;

    gl_Position = projection * viewPosition;

    vec4 normalTransformed = instanceMatrix * vec4(vertexNormal, 0.0);
    fragmentNormal = normalTransformed.xyz;

    fragmentUV = vertexUV;
    fragmentDepth = -viewPosition.z / farPlaneDistance;
    fragmentPosition = instancePosition.xyz;
}
