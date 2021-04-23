#version 450

#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable


layout(set = 1, binding = 1) uniform texture3D SurfaceTexture;
layout(set = 1, binding = 2) uniform sampler SurfaceSampler;

layout(location = 0) in vec3 fsin_0;
layout(location = 0) out vec4 fsout_color;

void main()
{
    fsout_color = texture(sampler3D(SurfaceTexture, SurfaceSampler), fsin_0);
}
