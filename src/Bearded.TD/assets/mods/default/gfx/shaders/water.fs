#version 150

uniform sampler2D depthBuffer;

uniform sampler2DArray noiseTexture;

uniform vec3 cameraPosition;
uniform float time;

in vec2 fragmentUV;
in vec3 fragmentPosition;
in vec3 fragmentNormal;
in vec2 fragmentFlow;
in vec3 pointOnFarPlane;

out vec4 fragColor;

float getCaustics(vec3 geometryPosition, float contrast)
{
	vec2 uv = geometryPosition.xy * 0.6;

    vec2 offset = vec2(time * 0.05, 0);

    vec4 noise = texture(noiseTexture, vec3((uv + offset) * 0.25, 0));
    vec4 noise2 = texture(noiseTexture, vec3((uv.yx + offset * 1.5) * 0.235, 0));
    vec4 noise3 = texture(noiseTexture, vec3((uv - vec2(offset.x * 1.2, offset.x * 1.2)) * -0.23, 0));

   	float n1 = max(0, 1 - abs(noise.x - 0.5) * contrast);
   	float n2 = max(0, 1 - abs(noise2.x - 0.5) * contrast);
   	float n3 = max(0, 1 - abs(noise3.x - 0.5) * contrast);

    float n = n2 * n1 * n3;

    return n * n;
}

void main()
{
    float depth = texture(depthBuffer, fragmentUV).x;

    vec3 geometryPositionRelativeToCamera = pointOnFarPlane * depth;
    vec3 geometryPosition = geometryPositionRelativeToCamera - cameraPosition;

    // caustics
    float c = getCaustics(geometryPosition, 6);
    vec4 causticHighlight = vec4(c, c, c, 0);

    // surface
    float surfaceHeight = getCaustics(fragmentPosition,  1)
    	* 5 / -cameraPosition.z;

    float surfaceDx = dFdx(surfaceHeight);
    float surfaceDy = dFdy(surfaceHeight);

    vec3 surfaceNormal = -vec3(surfaceDx, surfaceDy,
    	sqrt(1 - surfaceDx * surfaceDx - surfaceDy * surfaceDy)
    	);

    vec3 lightDir = normalize(vec3(1, -1, 3));

    float lightSurfaceDot = -dot(lightDir, surfaceNormal);

    float diffuseSurfaceLight = lightSurfaceDot;
    vec3 surfaceColor = vec3(0.2, 0.25, 0.5) * diffuseSurfaceLight;

    // why is this normalize needed?
    vec3 reflectedLightDir = normalize(lightDir + 2 * lightSurfaceDot * surfaceNormal);

    // why is this needed?
    vec3 c2 = vec3(-cameraPosition.xy, cameraPosition.z);

    vec3 fragmentToCamera = normalize(c2 - fragmentPosition);

    float specularSurfaceLight = dot(reflectedLightDir, fragmentToCamera);

    float specularHighlight = max(0, pow(specularSurfaceLight, 10));

    // compose
    float distanceToGeometry = length(geometryPosition - fragmentPosition);

    float transparency = clamp(distanceToGeometry * 0.5, 0.4, 0.8);
    float opacity = 1 - transparency;

    float causticVisibility = min(1, distanceToGeometry * 2) * 0.5 * opacity;

    fragColor =
    	causticHighlight * causticVisibility
    	+ vec4(surfaceColor, 1) * transparency
    	+ vec4(specularHighlight, specularHighlight, specularHighlight, 0) * 0.5;

    //float argb = specularHighlight;
    //fragColor = vec4(argb, argb, argb, 1);
}