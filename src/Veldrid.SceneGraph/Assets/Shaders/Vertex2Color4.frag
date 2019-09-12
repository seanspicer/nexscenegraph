//#version 450
//
//layout(location = 0) in vec4 fsin_Color;
//layout(location = 0) out vec4 OutputColor;
//
//void main()
//{
//    OutputColor = vec4(1.0, 1.0, 1.0, 1.0);
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


vec4 FS( Veldrid_SceneGraph_Shaders_Vertex2Color4ShaderSource_FragmentInput input_)
{
    return input_.Color;
}


layout(location = 0) in vec4 fsin_0;
layout(location = 0) out vec4 _outputColor_;

void main()
{
    Veldrid_SceneGraph_Shaders_Vertex2Color4ShaderSource_FragmentInput input_;
    input_.Position = gl_FragCoord;
    input_.Color = fsin_0;
    vec4 output_ = FS(input_);
    _outputColor_ = output_;
}
