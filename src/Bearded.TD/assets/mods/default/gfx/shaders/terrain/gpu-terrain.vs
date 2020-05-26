#version 150

uniform sampler2D heightmap;

uniform mat4 projection;
uniform mat4 view;

uniform float farPlaneDistance;

uniform float heightmapRadius;

uniform float heightmapPixelSizeUV;

in vec3 vertexPosition;
in vec3 vertexNormal;
in vec2 vertexUV;
in vec4 vertexColor;

out vec3 fragmentPosition;
out vec3 fragmentNormal;
out vec2 fragmentUV;
out vec4 fragmentColor;
out float fragmentDepth;

void main()
{
    vec3 p = vertexPosition;

    vec2 heightMapUV =
        p.xy / heightmapRadius // -1..1
        * 0.5 + 0.5; // 0..1

    float height = texture(heightmap, heightMapUV).x;

    p.z += height;

	vec4 viewPosition = view * vec4(p, 1.0);
	vec4 position = projection * viewPosition;
    gl_Position = position;


    float slopeX = texture(heightmap, heightMapUV + vec2(heightmapPixelSizeUV, 0)).x
        - texture(heightmap, heightMapUV - vec2(heightmapPixelSizeUV, 0)).x;
    float slopeY = texture(heightmap, heightMapUV + vec2(0, heightmapPixelSizeUV)).x
        - texture(heightmap, heightMapUV - vec2(0, heightmapPixelSizeUV)).x;

    float step = heightmapPixelSizeUV * heightmapRadius * 4;

    vec3 tangentX = vec3(step, 0, slopeX);
    vec3 tangentY = vec3(0, step, slopeY);

    vec3 normal = normalize(cross(tangentX, tangentY));


    
    fragmentPosition = p;
    fragmentNormal = normal;
    fragmentUV = vertexUV;
    fragmentColor = vertexColor;

    // check if this is actually in 0-1 space between camera and far plane
    // it probably is not because we don't take near distance into account properly
    float depth = -viewPosition.z / farPlaneDistance;
    fragmentDepth = depth;
}
