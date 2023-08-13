#version 150

uniform sampler2D biomemap;

uniform sampler2DArray diffuseFloor;
uniform sampler2DArray diffuseCrossSection;
uniform sampler2DArray diffuseWall;
uniform sampler2DArray normalFloor;
uniform sampler2DArray normalCrossSection;
uniform sampler2DArray normalWall;

uniform float farPlaneDistance;
uniform vec3 cameraPosition;

uniform mat4 view;

uniform float heightmapRadius;
uniform float heightScale;

in vec3 fragmentPosition;
in vec3 fragmentNormal;
in vec4 fragmentColor;

out vec4 outRGBA;
out vec4 outNormal;
out vec4 outDepth;

const float uvScale = 0.2;

void getWallXComponent(uint biomeId, vec3 position, vec3 surfaceNormal, out vec3 wallColor, out vec3 wallNormal)
{
    vec2 uv = position.xz * uvScale;
    uv.x = uv.x * -sign(surfaceNormal.y);

    vec3 zTransform = surfaceNormal;
    vec3 upVector = vec3(0, 0, 1);
    vec3 xTransform = normalize(cross(upVector, zTransform));
    vec3 yTransform = cross(xTransform, zTransform);

    vec3 rgb = texture(diffuseWall, vec3(uv, biomeId)).rgb;

    vec3 textureNormal = texture(normalWall, vec3(uv, biomeId))
        .xyz * 2 - 1;

    vec3 n = textureNormal.x * xTransform
           + textureNormal.y * yTransform
           + textureNormal.z * zTransform;

    wallColor = rgb;
    wallNormal = n;
}

void getWallYComponent(uint biomeId, vec3 position, vec3 surfaceNormal, out vec3 wallColor, out vec3 wallNormal)
{
    vec2 uv = position.yz * uvScale;
    uv.x = uv.x * sign(surfaceNormal.x);

    vec3 zTransform = surfaceNormal;
    vec3 upVector = vec3(0, 0, 1);
    vec3 xTransform = normalize(cross(upVector, zTransform));
    vec3 yTransform = cross(xTransform, zTransform);

    vec3 rgb = texture(diffuseWall, vec3(uv, biomeId)).rgb;

    vec3 textureNormal = texture(normalWall, vec3(uv, biomeId))
        .xyz * 2 - 1;

    vec3 n = textureNormal.x * xTransform
           + textureNormal.y * yTransform
           + textureNormal.z * zTransform;

    wallColor = rgb;
    wallNormal = n;
}

void getWallColor(uint biomeId, vec3 position, vec3 normal, out vec3 wallColor, out vec3 wallNormal)
{
    vec3 xColor, yColor;
    vec3 xNormal, yNormal;
    
    if(position.z > 0)
    {
        position.z *= 2;
    }
    position.z += sin(position.y * 0.2) * sin(position.x * 0.2) * 1;
    
    getWallXComponent(biomeId, position, normal, xColor, xNormal);
    getWallYComponent(biomeId, position, normal, yColor, yNormal);

    // TODO: this is not symetrical!
    float xness = clamp(abs(normal.x / normal.y), 0, 1);
    
    //xness = smoothstep(0.3, 0.7, xness);

    wallColor = mix(xColor, yColor, xness);
    wallNormal = mix(xNormal, yNormal, xness);
}

void getFloorColor(uint biomeId, vec3 position, vec3 surfaceNormal, out vec3 floorColor, out vec3 floorNormal)
{
    vec2 uvR = position.xy * uvScale;
    vec2 uvS = position.xy * uvScale;
    // distortion needs to be accounted for with normals
    // + vec2(0, sin(position.x * 0.2) * 2);

    vec3 rock = texture(diffuseCrossSection, vec3(uvR, biomeId)).rgb;
    vec3 sand = texture(diffuseFloor, vec3(uvS, biomeId)).rgb;

    float rockiness = clamp((position.z - 0.5) * 20, 0, 1);

    floorColor = mix(sand, rock - 0.25, rockiness);
    vec3 rockN = texture(normalCrossSection, vec3(uvR, biomeId)).rgb * 2 - 1;
    vec3 sandN = texture(normalFloor, vec3(uvS, biomeId)).rgb * 2 - 1;

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

float dither(vec2 xy)
{
    return fract(dot(xy, vec2(36, 7) / 16.0f));
}

void main()
{
    float limit = 0.75;
    float overhangLimit = 0.9;

    vec3 camPosition = -cameraPosition;
    vec3 cutoutCenter = vec3(camPosition.xy, camPosition.z * camPosition.z * 3);
    float cutoutRadius = cutoutCenter.z - limit;

    vec3 cutoutCenterToFragment = fragmentPosition - cutoutCenter;

    float distanceToCutoutCenter = length(cutoutCenterToFragment);

    if (heightScale > 0)
    {
        if (fragmentPosition.z > limit)
        {
            // discard top of regular terrain
            discard;
        }
    }
    else
    {
        if (distanceToCutoutCenter < cutoutRadius)
        {

            // uncomment for much more dithered fading
            //if (gl_FrontFacing)
            {
                // discard excessive height of upside down terrain
                discard;
            }
        }
    }

    vec3 fPosition = fragmentPosition;
    vec3 fNormal = fragmentNormal;
    vec4 fColor = fragmentColor;

    if(!gl_FrontFacing)
    {
        vec3 cutoutCenterToFragmentNormalised =
            cutoutCenterToFragment / distanceToCutoutCenter;

        vec3 camToFragment = camPosition - fragmentPosition;

        // intersect with cutout sphere
        vec3 oc = camPosition - cutoutCenter;
        float a = dot(camToFragment, camToFragment);
        float b = 2 * dot(oc, camToFragment);
        float c = dot(oc, oc) - cutoutRadius * cutoutRadius;
        float discriminant = b*b - 4*a*c;

        float f = (-b - sqrt(discriminant)) / (2.0*a);

        // properties of point on cutout sphere
        fPosition = camPosition + camToFragment * f;
        fNormal = -cutoutCenterToFragmentNormalised;
        fColor = vec4(fColor.rgb, fColor.a);

        if (heightScale < 0 && fragmentPosition.z > limit)
        {
            float d = dither(gl_FragCoord.xy);

            if (d < (fragmentPosition.z - limit) / (overhangLimit - limit))
            {
                discard;
            }
        }
    }
    
    vec2 biomeMapUV =
        fPosition.xy / heightmapRadius // -1..1
        * 0.5 + 0.5; // 0..1
    uint biomeId = uint(round(texture(biomemap, biomeMapUV).r * 255));

    vec3 wallColor, wallNormal;
    getWallColor(biomeId, fPosition, fNormal, wallColor, wallNormal);

    vec3 floorColor, floorNormal;
    getFloorColor(biomeId, fPosition, fNormal, floorColor, floorNormal);

    float flatness = smoothstep(0.3, 0.5, fNormal.z);

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

    vec4 rgba = vec4(diffuse, 1) * fColor;

    outRGBA = rgba;
    outNormal = vec4(normal * 0.5 + 0.5, rgba.a);

    // check if this is actually in 0-1 space between camera and far plane
    // it probably is not because we don't take near distance into account properly
    float depth = -(view * vec4(fPosition, 1)).z / farPlaneDistance;
    outDepth = vec4(depth, 0, 0, rgba.a);
}
