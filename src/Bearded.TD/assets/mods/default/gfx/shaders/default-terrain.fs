#version 150

uniform sampler2DArray diffuseTextures;
uniform sampler2DArray normalTextures;

in vec3 fragmentPosition;
in vec3 fragmentNormal;
in vec2 fragmentUV;
in vec4 fragmentColor;
in float fragmentDepth;

out vec4 outRGBA;
out vec4 outNormal;
out vec4 outDepth;

void getWallXComponent(vec3 position, vec3 surfaceNormal, out vec3 wallColor, out vec3 wallNormal)
{
    int textureIndex = 0;

    vec2 uv = position.xz / 1;
    uv.x = uv.x * -sign(surfaceNormal.y);

    vec3 zTransform = surfaceNormal;
    vec3 upVector = vec3(0, 0, 1);
    vec3 xTransform = normalize(cross(upVector, zTransform));
    vec3 yTransform = cross(xTransform, zTransform);

    vec3 rgb = texture(diffuseTextures, vec3(uv, textureIndex)).rgb;

    vec3 textureNormal = texture(normalTextures, vec3(uv, textureIndex))
        .xyz * 2 - 1;

    vec3 n = textureNormal.x * xTransform
           + textureNormal.y * yTransform
           + textureNormal.z * zTransform;
    
    wallColor = rgb;
    wallNormal = n;
}

void getWallYComponent(vec3 position, vec3 surfaceNormal, out vec3 wallColor, out vec3 wallNormal)
{
    int textureIndex = 0;

    vec2 uv = position.yz / 1;
    uv.x = uv.x * sign(surfaceNormal.x);

    vec3 zTransform = surfaceNormal;
    vec3 upVector = vec3(0, 0, 1);
    vec3 xTransform = normalize(cross(upVector, zTransform));
    vec3 yTransform = cross(xTransform, zTransform);

    vec3 rgb = texture(diffuseTextures, vec3(uv, textureIndex)).rgb;

    vec3 textureNormal = texture(normalTextures, vec3(uv, textureIndex))
        .xyz * 2 - 1;

    vec3 n = textureNormal.x * xTransform
           + textureNormal.y * yTransform
           + textureNormal.z * zTransform;
    
    wallColor = rgb;
    wallNormal = n;
}

void getWallColor(vec3 position, vec3 normal, out vec3 wallColor, out vec3 wallNormal)
{
    vec3 xColor, yColor;
    vec3 xNormal, yNormal;

    getWallXComponent(position, normal, xColor, xNormal);
    getWallYComponent(position, normal, yColor, yNormal);

    float xness = clamp(abs(normal.x / normal.y), 0, 1);

    xness = smoothstep(0.3, 0.7, xness);

    wallColor = mix(xColor, yColor, xness);
    wallNormal = mix(xNormal, yNormal, xness);
}

void getFloorColor(vec3 position, vec3 surfaceNormal, out vec3 floorColor, out vec3 floorNormal)
{
    int rockIndex = 1;
    int sandIndex = 2;

    vec2 uvR = position.xy / 3;
    vec2 uvS = position.xy / 1.7;
    // distortion needs to be accounted for with normals
    // + vec2(0, sin(position.x * 0.2) * 2);

    vec3 rock = texture(diffuseTextures, vec3(uvR, rockIndex)).rgb;
    vec3 sand = texture(diffuseTextures, vec3(uvS, sandIndex)).rgb;

    float rockiness = clamp(position.z * 20, 0, 1);

    floorColor = mix(sand, rock - 0.25, rockiness);

    vec3 rockN = texture(normalTextures, vec3(uvR, rockIndex)).rgb * 2 - 1;
    vec3 sandN = texture(normalTextures, vec3(uvS, sandIndex)).rgb * 2 - 1;

    vec3 textureNormal = normalize(mix(sandN, rockN, rockiness));

    vec3 zTransform = surfaceNormal;
    vec3 upVector = vec3(0, 1, 0);
    vec3 xTransform = normalize(cross(upVector, zTransform));
    vec3 yTransform = cross(xTransform, zTransform);

    vec3 n = textureNormal.x * xTransform
           + textureNormal.y * yTransform
           + textureNormal.z * zTransform;

    floorNormal = n;
}


void main()
{
    vec3 wallColor, wallNormal;
    getWallColor(fragmentPosition, fragmentNormal, wallColor, wallNormal);

    vec3 floorColor, floorNormal;
    getFloorColor(fragmentPosition, fragmentNormal, floorColor, floorNormal);
    
    float flatness = smoothstep(0.6, 0.9, fragmentNormal.z);

    vec3 diffuse, normal;

    if (flatness > 0.999)
    {
        diffuse = floorColor;
        normal = floorNormal;
    }
    else
    {

        diffuse = mix(wallColor, floorColor, flatness);
        normal = mix(wallNormal, floorNormal, flatness);
    }
    
    vec4 rgba = vec4(diffuse, 1) * fragmentColor;

    outRGBA = rgba;
    outNormal = vec4(normal * 0.5 + 0.5, rgba.a);
    outDepth = vec4(fragmentDepth, 0, 0, rgba.a);
}
