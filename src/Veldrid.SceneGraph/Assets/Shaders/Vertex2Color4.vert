//#version 450
//
//layout(location = 0) in vec2 Position;
//layout(location = 1) in vec4 Color;
//layout(location = 0) out vec4 fsin_0;
//
//layout(std140) uniform Projection
//{
//    mat4 field_Projection;
//};
//
//layout(std140) uniform View
//{
//    mat4 field_View;
//};
//
//layout(std140) uniform Model
//{
//    mat4 field_Model;
//};
//
//void main()
//{
//    fsin_0 = Color;
//    vec4 outPos = field_Projection * field_View * field_Model * vec4(Position.x, Position.y, 0.0f, 1.f);
//    gl_Position = outPos;
//    gl_Position.z = gl_Position.z * 2.0 - gl_Position.w;
//}

#version 450

struct SamplerDummy { int _dummyValue; };
struct SamplerComparisonDummy { int _dummyValue; };

struct Veldrid_SceneGraph_Shaders_Vertex2Color4ShaderSource_VertexInput
{
    vec2 Position;
    vec4 Color;
};

struct Veldrid_SceneGraph_Shaders_Vertex2Color4ShaderSource_FragmentInput
{
    vec4 Position;
    vec4 Color;
};

layout(std140) uniform Projection
{
    mat4 field_Projection;
};

layout(std140) uniform View
{
    mat4 field_View;
};

layout(std140) uniform Model
{
    mat4 field_Model;
};


Veldrid_SceneGraph_Shaders_Vertex2Color4ShaderSource_FragmentInput VS( Veldrid_SceneGraph_Shaders_Vertex2Color4ShaderSource_VertexInput input_)
{
    Veldrid_SceneGraph_Shaders_Vertex2Color4ShaderSource_FragmentInput output_;
    output_.Color = input_.Color;
    output_.Position = field_Projection * field_View * field_Model * vec4(input_.Position.x, input_.Position.y, 0.0f, 1.f);
    return output_;
}


layout(location = 0) in vec2 Position;
layout(location = 1) in vec4 Color;
layout(location = 0) out vec4 fsin_0;

void main()
{
    Veldrid_SceneGraph_Shaders_Vertex2Color4ShaderSource_VertexInput input_;
    input_.Position = Position;
    input_.Color = Color;
    Veldrid_SceneGraph_Shaders_Vertex2Color4ShaderSource_FragmentInput output_ = VS(input_);
    fsin_0 = output_.Color;
    gl_Position = output_.Position;
    gl_Position.z = gl_Position.z * 2.0 - gl_Position.w;
}