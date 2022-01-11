#version 150

uniform sampler2D heightmap;

uniform mat4 projection;
uniform mat4 view;

uniform float heightmapRadius;

uniform float heightmapPixelSizeUV;

uniform float heightScale;
uniform float heightOffset;

uniform vec2 gridOffset;
uniform vec2 gridScale;

in vec3 vertexPosition;
in vec3 vertexNormal; // not used
in vec2 vertexUV; // not used
in vec4 vertexColor; // used?

out vec3 fragmentPosition;
out vec3 fragmentNormal;
//out vec2 fragmentUV;
out vec4 fragmentColor;

void main()
{
    vec3 p = vertexPosition;

    p.xy = p.xy * gridScale + gridOffset;

    vec2 heightMapUV =
        p.xy / heightmapRadius // -1..1
        * 0.5 + 0.5; // 0..1

    vec4 heightMapValue = texture(heightmap, heightMapUV);

    float height = heightMapValue.x;

    float visibility = heightMapValue.y;

    height = mix(1, height, visibility);

    height = height * heightScale + heightOffset;

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
    //fragmentUV = vertexUV;
    fragmentColor = vertexColor;
}
