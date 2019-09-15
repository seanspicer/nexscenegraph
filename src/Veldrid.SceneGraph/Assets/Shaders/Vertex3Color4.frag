#version 450

#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable

struct FragmentInput
{
    vec4 Position;
    vec4 Color;
};

vec4 CalculateColor(FragmentInput input_) 
{
    return input_.Color;
}

layout(location = 0) in vec4 fsin_0;
layout(location = 0) out vec4 OutputColor;

void main()
{
    FragmentInput input_;
    input_.Position = gl_FragCoord;
    input_.Color = fsin_0;
    vec4 output_ = CalculateColor(input_);
    OutputColor = output_;
}
