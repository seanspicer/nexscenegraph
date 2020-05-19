#version 450

#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable

struct FragmentInput
{
    vec4 SystemPosition;
    vec2 TexCoords;
};

layout(set = 1, binding = 1) uniform texture2D SurfaceTexture;
layout(set = 1, binding = 2) uniform sampler SurfaceSampler;

vec4 ComputeFragmentColor( FragmentInput input_)
{
    return texture(sampler2D(SurfaceTexture, SurfaceSampler), input_.TexCoords);
}

layout(location = 0) in vec2 fsin_0;
layout(location = 0) out vec4 _outputColor_;

void main()
{
    FragmentInput input_;
    input_.SystemPosition = gl_FragCoord;
    input_.TexCoords = fsin_0;
    vec4 output_ = ComputeFragmentColor(input_);
    _outputColor_ = output_;
}
