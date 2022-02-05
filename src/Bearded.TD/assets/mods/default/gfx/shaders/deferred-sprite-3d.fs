#version 150

uniform sampler2D diffuse;
uniform sampler2D normal;

in vec3 fragmentNormal;
in vec3 fragmentTangent;
in vec2 fragmentUV;
in vec4 fragmentColor;
in float fragmentDepth;

out vec4 outRGBA;
out vec4 outNormal;
out vec4 outDepth;

void main()
{
	vec4 rgba = texture(diffuse, fragmentUV);

	if (rgba.a < 0.01)
		discard;

	vec3 normal = texture(normal, fragmentUV).xyz * 2 - 1;

	vec3 fNormal = normalize(fragmentNormal);
	vec3 fTangent = normalize(fragmentTangent);
	vec3 fBiTangent = cross(fNormal, fTangent);

	vec3 rotatedNormal = normal.x * fTangent + normal.y * fBiTangent + normal.z * fNormal;

    outRGBA = rgba;
    outNormal = vec4(rotatedNormal * 0.5 + 0.5, 1) * rgba.a;
    outDepth = vec4(fragmentDepth, 0, 0, 1) *  rgba.a;
}
