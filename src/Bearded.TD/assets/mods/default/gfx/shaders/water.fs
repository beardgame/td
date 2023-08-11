#version 150

uniform sampler2D depthBuffer;

uniform sampler2D noiseTexture;

uniform vec3 cameraPosition;
uniform float time;

in vec2 fragmentUV;
in vec3 fragmentPosition;
in vec3 fragmentNormal;
in vec2 fragmentFlow;
in vec3 pointOnFarPlane;

out vec4 fragColor;


const float cyclePeriod = 2;
const float waveSizeInverse = 0.5;

const float flowFactor = 40;
const float maxFlow = 1.2;

const float minOpacity = 0.2;
const float maxOpacity = 0.8;
const float opacityFalloff = 2;


float getCaustics(vec3 geometryPosition, vec2 flow, float contrast, float timeOffset)
{
	vec2 uv = geometryPosition.xy * waveSizeInverse
        + vec2(timeOffset, timeOffset);

    vec2 offset = vec2(time * 0.05, 0);

    float t = time / cyclePeriod + timeOffset;
    t = t - floor(t) - 0.5;

    uv += flow * t;

    vec4 noise1 = texture(noiseTexture,
        (uv + offset) * 0.25
        );
    vec4 noise2 = texture(noiseTexture,
        (uv.yx + offset * 1.5) * 0.235
        );

   	float n1 = max(0, 1 - abs(noise1.x - 0.5) * contrast);
   	float n2 = max(0, 1 - abs(noise2.x - 0.5) * contrast);
    float n = n1 * n2;

    return n * n * n;
}

void main()
{
    float depth = texture(depthBuffer, fragmentUV).x;

    vec3 geometryPositionRelativeToCamera = pointOnFarPlane * depth;
    vec3 geometryPosition = geometryPositionRelativeToCamera - cameraPosition;

    float t = time / cyclePeriod;
    float cycleLerpF = abs((t - floor(t)) *  2 - 1);

    vec2 flow = -fragmentFlow * flowFactor;

    float flowStrength = length(flow);
    if (flowStrength > maxFlow)
    {
        flow = flow / flowStrength * maxFlow;
        flowStrength = maxFlow;
    }

    // caustics
    float caustic1 = getCaustics(geometryPosition, flow, 6, 0);
    float caustic2 = getCaustics(geometryPosition, flow, 6, 0.5);

    float c = mix(caustic1, caustic2, cycleLerpF);
    vec4 causticHighlight = vec4(c, c, c, 0);

    // surface
    float height1 = getCaustics(fragmentPosition, flow,  1, 0);
    float height2 = getCaustics(fragmentPosition, flow,  1, 0.5);
    float height = mix(height1, height2, cycleLerpF);

    float surfaceHeight = height * (0.5 + flowStrength)
     * 5 / -cameraPosition.z;

    float surfaceDx = dFdx(surfaceHeight);
    float surfaceDy = dFdy(surfaceHeight);

    vec3 surfaceNormal = -vec3(surfaceDx, surfaceDy,
    	sqrt(1 - surfaceDx * surfaceDx - surfaceDy * surfaceDy)
    	);

    vec3 lightDir = normalize(vec3(1, -1, 3));

    float lightSurfaceDot = -dot(lightDir, surfaceNormal);

    float diffuseSurfaceLight = lightSurfaceDot;
    vec3 surfaceColor = vec3(0.2, 0.2, 0.3) * diffuseSurfaceLight;

    // why is this normalize needed?
    vec3 reflectedLightDir = normalize(lightDir + 2 * lightSurfaceDot * surfaceNormal);

    // why is this needed?
    vec3 c2 = vec3(-cameraPosition.xy, cameraPosition.z);

    vec3 fragmentToCamera = normalize(c2 - fragmentPosition);

    float specularSurfaceLight = dot(reflectedLightDir, fragmentToCamera);

    float specularHighlight = max(0, pow(specularSurfaceLight, 10));

    // foam
    float foam = 1 - (-surfaceNormal.z*101 - 100);


    // compose
    float distanceToGeometry = length(geometryPosition - fragmentPosition);


    float transparency = clamp(distanceToGeometry * opacityFalloff, minOpacity, maxOpacity);
    float opacity = 1 - transparency;

    float causticVisibility = min(1, distanceToGeometry * 2) * 0.5 * opacity;

    fragColor =
    	causticHighlight * causticVisibility +
    	vec4(surfaceColor, 1) * transparency +
    	vec4(specularHighlight, specularHighlight, specularHighlight * 1.2, 0) * 0.5 +
        vec4(foam, foam, foam, foam) * 0.5;
}
