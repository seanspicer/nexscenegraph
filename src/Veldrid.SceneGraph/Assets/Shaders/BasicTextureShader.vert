#version 450
#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable

struct VertexInput
{
    vec3 Position;
    vec2 TexCoords;
};

struct FragmentInput
{
    vec4 SystemPosition;
    vec2 TexCoords;
};

layout(set = 0, binding = 0) uniform Projection
{
    mat4 field_Projection;
};

layout(set = 0, binding = 1) uniform View
{
    mat4 field_View;
};

layout(set = 1, binding = 0) uniform Model
{
    mat4 field_Model;
};


FragmentInput ComputeFragmentInput( VertexInput input_)
{
    FragmentInput output_;
    vec4 worldPosition = field_Model * vec4(input_.Position, 1);
    vec4 viewPosition = field_View * worldPosition;
    vec4 clipPosition = field_Projection * viewPosition;
    output_.SystemPosition = clipPosition;
    output_.TexCoords = input_.TexCoords;
    return output_;
}

layout(location = 0) in vec3 Position;
layout(location = 1) in vec2 TexCoords;
layout(location = 0) out vec2 fsin_0;

void main()
{
    VertexInput input_;
    input_.Position = Position;
    input_.TexCoords = TexCoords;
    FragmentInput output_ = ComputeFragmentInput(input_);
    fsin_0 = output_.TexCoords;
    gl_Position = output_.SystemPosition;
}