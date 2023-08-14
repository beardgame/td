#version 400

layout(triangles) in;
layout(triangle_strip, max_vertices = 3) out;

in float geometryBiomeId[];

out vec3 fragmentBiomeId;
out vec3 baricentricCoords;

void main() {
    fragmentBiomeId = vec3(
    	geometryBiomeId[0], geometryBiomeId[1], geometryBiomeId[2]
	);
    for (int i = 0; i < 3; i++) {
        gl_Position = gl_in[i].gl_Position;
        baricentricCoords = vec3(0);
        baricentricCoords[i] = 1;
        EmitVertex();
    }
    EndPrimitive();
}
