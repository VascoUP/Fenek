
#version 450

#extension GL_ARB_gpu_shader_int64 : enable
#extension GL_NV_shader_atomic_int64 : enable

layout(local_size_x = 128, local_size_y = 1) in;

struct Vertex{
	float x;
	float y;
	float z;
	uint colors;
	//float ar;
	//float ag;
	//float ab;
	//float aw;
};

layout(location = 0) uniform mat4 uTransform;

layout (std430, binding = 0) buffer point_data {
	Vertex vertices[];
};

layout (std430, binding = 1) buffer framebuffer_data {
	//uint ssFramebuffer[];
	uint64_t ssFramebuffer[];
};

uniform ivec2 uImageSize;


void main(){

	uint workGroupSize = gl_WorkGroupSize.x * gl_WorkGroupSize.y;
	uint globalID = gl_WorkGroupID.x * workGroupSize
		+ gl_WorkGroupID.y * gl_NumWorkGroups.x * workGroupSize
		+ gl_LocalInvocationIndex;

	//globalID += 110 * 1000 * 1000;

	Vertex v = vertices[globalID];

	vec4 pos = uTransform * vec4(v.x, v.y, v.z, 1.0);
	pos.xyz = pos.xyz / pos.w;

	if(pos.w <= 0.0 || pos.x < -1.0 || pos.x > 1.0 || pos.y < -1.0 || pos.y > 1.0){
		return;
	}

	vec2 imgPos = (pos.xy * 0.5 + 0.5) * uImageSize;
	ivec2 pixelCoords = ivec2(imgPos);
	int pixelID = pixelCoords.x + pixelCoords.y * uImageSize.x;

	float depth = pos.w;
	uint64_t u64Depth = uint64_t(depth * 1000000.0);

	uint64_t u64Colors = v.colors;
	uint64_t val64 = (u64Depth << 24) | v.colors;


	atomicMin(ssFramebuffer[pixelID], val64);
	//atomicAdd(ssFramebuffer[pixelID], 1u);

}

