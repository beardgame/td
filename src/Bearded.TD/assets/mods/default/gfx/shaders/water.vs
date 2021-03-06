#version 150

uniform mat4 projection;
uniform mat4 view;

uniform vec3 farPlaneBaseCorner;
uniform vec3 farPlaneUnitX;
uniform vec3 farPlaneUnitY;

in vec3 vertexPosition;
in vec3 vertexNormal;
in vec2 vertexFlow;

out vec2 fragmentUV;
out vec3 fragmentPosition;
out vec3 fragmentNormal;
out vec2 fragmentFlow;
out vec3 pointOnFarPlane;

void main()
{
    vec4 p = projection * view * vec4(vertexPosition, 1.0);
    gl_Position = p;

    fragmentUV = (p.xy / p.w + 1) * 0.5;
    

    // interpolation along the diagonal might be broken?
    // probably only for weird geometry
    pointOnFarPlane = farPlaneBaseCorner
        + farPlaneUnitX * fragmentUV.x
        + farPlaneUnitY * fragmentUV.y;
    
    fragmentPosition = vertexPosition;
    fragmentNormal = vertexNormal;
    fragmentFlow = vertexFlow;
}
