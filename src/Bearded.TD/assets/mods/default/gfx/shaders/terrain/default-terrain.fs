#version 150

uniform isampler2D biomeTilemap;
uniform int biomeTilemapRadius;

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

const float wallUvScale = 1;
const float floorUvScale = 0.5;
const float insideBiomeBlendHardness = 0.3; // 0.33 max!
const float edgeBiomeBlendHardness = 0;

#define BOMBING true

void getWallXComponent(int biomeId, vec3 position, vec3 surfaceNormal, out vec3 wallColor, out vec3 wallNormal)
{
    vec2 uv = position.xz * wallUvScale;
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

void getWallYComponent(int biomeId, vec3 position, vec3 surfaceNormal, out vec3 wallColor, out vec3 wallNormal)
{
    vec2 uv = position.yz * wallUvScale;
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

void getWallColor(int biomeId, vec3 position, vec3 normal, out vec3 wallColor, out vec3 wallNormal)
{
    vec3 xColor, yColor;
    vec3 xNormal, yNormal;

    vec3 uv = position * wallUvScale;
    
    uv.z *= -0.5;
    
    if(uv.z < 0)
    {
        uv.z *= 0.5;
    }
    uv.z += sin(uv.y * 0.2) * sin(uv.x * 0.2) * 1;
    
    getWallXComponent(biomeId, uv, normal, xColor, xNormal);
    getWallYComponent(biomeId, uv, normal, yColor, yNormal);

    // TODO: this is not symetrical!
    float xness = clamp(abs(normal.x / normal.y), 0, 1);
    
    //xness = smoothstep(0.3, 0.7, xness);

    wallColor = mix(xColor, yColor, xness);
    wallNormal = mix(xNormal, yNormal, xness);
}

void getFloorColor(int biomeId, vec3 position, vec3 surfaceNormal, out vec3 floorColor, out vec3 floorNormal)
{
    vec2 uvR = position.xy * floorUvScale;
    vec2 uvS = position.xy * floorUvScale;
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

void getWallAndFloor(int biomeId, vec3 position, float uvRotationAngle, vec3 normal, float flatness, out vec3 wallColor, out vec3 wallNormal, out vec3 floorColor, out vec3 floorNormal)
{
    if (flatness < 1)
        getWallColor(biomeId, position, normal, wallColor, wallNormal);
    if (flatness > 0)
    {
        vec3 uvw = position;
        mat2 uvRotation;
        if (BOMBING)
        {
            float s = sin(uvRotationAngle);
            float c = cos(uvRotationAngle);
            uvRotation = mat2(c, -s, s, c);
            uvw.xy = uvRotation * uvw.xy;
            normal.xy = uvRotation * normal.xy;
        }
        
        getFloorColor(biomeId, uvw, normal, floorColor, floorNormal);

        if (BOMBING)
        {
            floorNormal.xy = transpose(uvRotation) * floorNormal.xy;
        }
    }
}

float dither(vec2 xy){ return fract(dot(xy, vec2(36, 7) / 16.0f)); }
float rand2(vec2 co){ return fract(sin(dot(co, vec2(12.9898, 78.233))) * 43758.5453); }
float rand1(float n){return fract(sin(n) * 43758.5453123);}

void getTile(vec2 xy, out ivec2 tile, out vec2 offset)
{
    const float hexDistanceY = 0.8660254;
    const float hexDistanceX = 1;
    
    float yf = xy.y * (1 / hexDistanceY) + 1 / 1.5;
    float y = floor(yf);
    float xf = xy.x * (1 / hexDistanceX) - y * 0.5 + 0.5;
    float x = floor(xf);

    float xRemainder = xf - x - 0.5;
    float yRemainder = (yf - y) * 1.5;

    tile = ivec2(x, y);

    bool isBottomRightCorner = xRemainder > yRemainder;
    bool isBottomLeftCorner = -xRemainder > yRemainder;

    tile.x += isBottomRightCorner ? 1 : 0;
    tile.y += isBottomRightCorner || isBottomLeftCorner ? -1 : 0;
    
    vec2 center = vec2(
        (tile.x + tile.y * 0.5f) * hexDistanceX,
        tile.y * hexDistanceY
    );
    
    offset = xy - center;
}

void getTileGridRhombus(vec2 xy, out ivec2 tile, out vec2 offset)
{
    const float hexDistanceY = 0.8660254;
    const float hexDistanceX = 1;
    
    float yf = xy.y * (1 / hexDistanceY);
    float xf = xy.x * (1 / hexDistanceX) - yf * 0.5;

    float y = floor(yf);
    float x = floor(xf);

    if (BOMBING)
    {
        if (dFdy(y) != 0)
        {
            y = floor(yf + 0.5);
        }
        if (dFdx(x) != 0)
        {
            x = floor(xf + 0.5);
        }
        if (dFdy(x) != 0)
        {
            x = floor(xf - 0.5);
        }
    }

    float yRemainder = yf - y;
    float xRemainder = xf - x;
    
    tile = ivec2(x, y);
    offset = vec2(xRemainder, yRemainder);
}

void getBiomeBlend(vec2 tileUV, ivec4 biomeData, out ivec3 biomeIds, out vec3 biomeWeights, out ivec2 thirdTileOffset)
{
    int b0, b1, b2;
    float w0, w1, w2;
    if(tileUV.x + tileUV.y <= 1)
    {
        b0 = biomeData[0];
        b1 = biomeData[2];
        b2 = biomeData[3];

        w0 = tileUV.y;
        w1 = tileUV.x;
        w2 = 1 - w0 - w1;
        thirdTileOffset = ivec2(0, -1);
    }
    else
    {
        b0 = biomeData[0];
        b1 = biomeData[2];
        b2 = biomeData[1];

        w0 = 1 - tileUV.x;
        w1 = 1 - tileUV.y;
        w2 = 1 - w0 - w1;
        thirdTileOffset = ivec2(1, 0);
    }
    
    if (BOMBING)
    {
        biomeIds = ivec3(b0, b1, b2);
        biomeWeights = vec3(w0, w1, w2);
    }
    else
    {
        biomeIds[0] = b0;
        biomeWeights[0] = w0;

        if (b1 == b0)
        {
            biomeWeights[0] += w1;
        }
        else
        {
            biomeIds[1] = b1;
            biomeWeights[1] = w1;
        }

        if (b2 == b0)
        {
            biomeWeights[0] += w2;
        }
        else if (b2 == b1)
        {
            biomeWeights[1] += w2;
        }
        else
        {
            biomeIds[2] = b2;
            biomeWeights[2] = w2;
        }
    }
    bool allSameBiome = b0 == b1 && b1 == b2;
    biomeWeights = smoothstep(
        allSameBiome ? insideBiomeBlendHardness : edgeBiomeBlendHardness,
        1, biomeWeights);
}

void getBiomeSampleData(vec2 xy, out ivec3 biomeIds, out vec3 biomeWeights, inout ivec2 tiles[3])
{
    ivec2 tile;
    vec2 tileUV;
    getTileGridRhombus(xy, tile, tileUV);

    ivec2 sampleTile = tile + ivec2(0, 1);
    vec2 biomeUV = vec2(sampleTile) / (biomeTilemapRadius * 2) + vec2(0.5);
    ivec4 biomeData = ivec4(texture(biomeTilemap, biomeUV));
    
    ivec2 thirdTileOffset;

    getBiomeBlend(tileUV, biomeData, biomeIds, biomeWeights, thirdTileOffset);

    tiles[0] = sampleTile;
    tiles[1] = sampleTile + ivec2(1, -1);
    tiles[2] = sampleTile + thirdTileOffset;
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

    ivec3 biomeIds;
    vec3 biomeWeights;
    ivec2 tiles[3];
    getBiomeSampleData(fPosition.xy, biomeIds, biomeWeights, tiles);
    
    vec3 wallColor, wallNormal;
    vec3 floorColor, floorNormal;

    float flatness = smoothstep(0.3, 0.5, fNormal.z);
    
    float weightSum = biomeWeights[0] + biomeWeights[1] + biomeWeights[2];
    
    for(int i = 0; i < 3; i++)
    {
        int biome = biomeIds[i];
        float weight = biomeWeights[i] / weightSum;
        
        if (!BOMBING && weight == 0)
            continue;

        vec3 uvw = fPosition;
        mat2 uvRotation;
        float uvRotationAngle;
        
        if (BOMBING)
        {
            vec2 randomUV = vec2(rand2(tiles[i].xy), rand2(tiles[i].yx));
            uvRotationAngle = rand2(tiles[i].xy * 51.18) * 6.2831;
            
            uvw.xy += randomUV;
        }
        
        vec3 wC, wN, fC, fN;
        getWallAndFloor(biome, uvw, uvRotationAngle, fNormal, flatness, wC, wN, fC, fN);
        
        wallColor += wC * weight;
        wallNormal += wN * weight;
        floorColor += fC * weight;
        floorNormal += fN * weight;
    }

    vec3 diffuse, normal;
    diffuse = mix(wallColor, floorColor, flatness);
    normal = mix(wallNormal, floorNormal, flatness);
    
    vec4 rgba = vec4(diffuse, 1) * fColor;
    
    outRGBA = rgba;
    outNormal = vec4(normal * 0.5 + 0.5, rgba.a);

    // check if this is actually in 0-1 space between camera and far plane
    // it probably is not because we don't take near distance into account properly
    float depth = -(view * vec4(fPosition, 1)).z / farPlaneDistance;
    outDepth = vec4(depth, 0, 0, rgba.a);
}
