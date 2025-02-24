#version 150

const float PI = 3.14159265359;

uniform sampler2D diffuseBuffer;
uniform sampler2D normalBuffer;
uniform sampler2D materialBuffer;
uniform sampler2D depthBuffer;

uniform vec2 resolution;

uniform vec3 farPlaneBaseCorner;
uniform vec3 farPlaneUnitX;
uniform vec3 farPlaneUnitY;
uniform vec3 cameraPosition;

in vec2 lightCenterUV;
in vec3 lightPosition;
in float lightRadiusSquared;
in vec4 lightColor;
in float fallOffPower;
in float lightShadow;

out vec4 outRGB;

vec3 getFragmentPositionFromDepth(vec2 uv)
{
    uv = clamp(uv, 0.001, 0.999);

    float depth = texture(depthBuffer, uv).x;

    vec3 pointOnFarPlane = farPlaneBaseCorner
        + farPlaneUnitX * uv.x
        + farPlaneUnitY * uv.y;

    vec3 fragmentPositionRelativeToCamera = pointOnFarPlane * depth;
    vec3 fragmentPosition = fragmentPositionRelativeToCamera - cameraPosition;

    return fragmentPosition;
}

bool trySolve(
    float quadratic, float linear, float constant,
    out float solution1, out float solution2
)
{
    float r = linear * linear - 4 * quadratic * constant;
    if (r < 0)
    {
        solution1 = 0;
        solution2 = 0;
        return false;
    }

    float root = sqrt(r);

    float q2 = 2 * quadratic;

    solution1 = (-linear + root) / q2;
    solution2 = (-linear - root) / q2;

    return true;
}

bool getPointsOnSphere(
    vec2 uv,
    out vec3 point1, out vec3 point2
)
{
    vec3 pointOnFarPlane = farPlaneBaseCorner
        + farPlaneUnitX * uv.x
        + farPlaneUnitY * uv.y;

    vec3 rayStart = cameraPosition;
    vec3 direction = -pointOnFarPlane;

    vec3 relativePosition = rayStart + lightPosition;
    float radiusSquared = lightRadiusSquared;
    float relativePositionSquared = dot(relativePosition, relativePosition);

    float quadratic = dot(direction, direction);
    float linear = 2 * dot(direction, relativePosition);
    float constant = relativePositionSquared - radiusSquared;

    float solution1;
    float solution2;

    if (trySolve(quadratic, linear, constant, solution1, solution2))
    {
        point1 = rayStart + direction * solution1;
        point2 = rayStart + direction * solution2;

        return true;
    }
    return false;
}

float dither(vec2 xy)
{
    return fract(dot(xy, vec2(36, 7) / 16.0f));
}

void main()
{
    vec2 uv = gl_FragCoord.xy / resolution;

    vec3 fragmentPosition = getFragmentPositionFromDepth(uv);

    vec3 point1;
    vec3 point2;

    if (!getPointsOnSphere(uv, point1, point2))
    {
        discard;
    }

    vec3 point1Normal = normalize(point1 + lightPosition);
    vec3 point2Normal = normalize(point2 + lightPosition);

    outRGB = vec4(point1Normal * 0.5 + 0.5, 1);
}
