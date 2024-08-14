#version 150

uniform mat4 projection;
uniform mat4 view;

in vec3 vertexPosition;
in vec3 vertexNormal;
in vec2 vertexUV;

in mat4 instanceMatrix;

out vec3 fragmentNormal;
out vec2 fragmentUV;
out float fragmentDepth;

void main()
{
    vec4 worldPosition = instanceMatrix * vec4(vertexPosition, 1.0);

    gl_Position = projection * view * vec4(vertexPosition, 1.0);

    vec4 normalTransformed = projection * view * instanceMatrix * vec4(vertexNormal, 0.0);
    fragmentNormal = normalTransformed.xyz / normalTransformed.w;

    fragmentUV = vertexUV;
    fragmentDepth = worldPosition.z;
}
