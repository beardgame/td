#version 150

uniform sampler2D diffuse;
uniform sampler2D normal;

in vec2 fragmentUV;
in vec4 fragmentColor;
in float fragmentDepth;

out vec4 outRGBA;
out vec4 outNormal;
out vec4 outDepth;

const bool showNormals = false;

void main()
{
	vec4 rgba = texture(diffuse, fragmentUV);
	
	if (showNormals)
	{
	    vec3 n = texture(normal, fragmentUV).xyz * 2 - 1;
        outRGBA = vec4(n * 0.5 + 0.5, 1);
        outNormal = vec4(0, 0, 1, 1);
        outDepth = vec4(fragmentDepth, 0, 0, 1);
        return;
    }

	if (rgba.a < 0.9)
		discard;
		
	vec3 normal = texture(normal, fragmentUV).xyz * 2 - 1;
	
	vec2 unitX = normalize(dFdx(fragmentUV));
	vec2 unitY = -normalize(dFdy(fragmentUV));

	vec3 rotatedNormal = vec3(unitX * normal.x + unitY * normal.y, normal.z);

    outRGBA = vec4(rgba.rgb * rgba.a, 1);
    outNormal = vec4(rotatedNormal * 0.5 + 0.5, 1);
    outDepth = vec4(fragmentDepth, 0, 0, 1);
    
}
