#version 450

#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable
struct VertexInput 
{
    vec3 Position;
    vec3 TexCoord;
    vec4 Color;
};

struct FragmentInput 
{
    vec4 Position;
    vec3 TexCoord;
    vec4 Color;
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

FragmentInput Calculate(VertexInput input_) 
{
    FragmentInput output_;
    output_.Color = input_.Color;
    output_.TexCoord = input_.TexCoord;
    output_.Position = field_Projection * field_View * field_Model * vec4(input_.Position, 1.f);
    return output_;
}

layout(location = 0) in vec3 Position;
layout(location = 1) in vec3 TexCoord;
layout(location = 2) in vec4 Color;
layout(location = 0) out vec4 fsin_0;
layout(location = 1) out vec3 fsin_1;

void main()
{
    VertexInput input_;
    input_.Position = Position;
    input_.TexCoord = TexCoord;
    input_.Color = Color;
    
    FragmentInput output_ = Calculate(input_);
    fsin_0 = output_.Color;
    fsin_1 = output_.TexCoord;
    gl_Position = output_.Position;
}
