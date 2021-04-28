#version 450

#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable

struct FragmentInput
{
    vec4 Position;
    vec3 TexCoord;
    vec4 Color;
};

//vec4 CalculateColor(FragmentInput input_) 
//{
//    return texture(sampler3D(SurfaceTexture, SurfaceSampler), input_.TexCoord);
//}

layout(set = 1, binding = 1) uniform texture3D SurfaceTexture;
layout(set = 1, binding = 2) uniform sampler SurfaceSampler;

layout(location = 0) in vec4 fsin_0;
layout(location = 1) in vec3 fsin_1;
layout(location = 0) out vec4 OutputColor;

void main()
{
    FragmentInput input_;
    input_.Position = gl_FragCoord;
    input_.Color = fsin_0;
    input_.TexCoord = fsin_1;
//    vec4 output_ = CalculateColor(input_);
    vec4 output_ = texture(sampler3D(SurfaceTexture, SurfaceSampler), input_.TexCoord);
    OutputColor = output_;
}