#version 450
#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable
struct MultiTexturedCube_Shaders_MultiTexturedCubeShader_VertexInput
{
    vec3 Position;
    vec2 TexCoords;
    vec4 Color;
};

struct MultiTexturedCube_Shaders_MultiTexturedCubeShader_FragmentInput
{
    vec4 SystemPosition;
    vec2 TexCoords;
    vec4 Color;
};

layout(set = 1, binding = 1) uniform texture2D SurfaceTexture;
layout(set = 1, binding = 2) uniform sampler SurfaceSampler;
layout(set = 1, binding = 3) uniform texture2D TreeTexture;
layout(set = 1, binding = 4) uniform sampler TreeSampler;
vec4 MultiTexturedCube_Shaders_MultiTexturedCubeShader_Over( vec4 a,  vec4 b)
{
    vec4 result = vec4(0, 0, 0, 0);
    result = a * a.w + b * b.w * (1 - a.w) / (a.w + b.w * 1 - a.w);
    return result;
}



vec4 FS( MultiTexturedCube_Shaders_MultiTexturedCubeShader_FragmentInput input_)
{
    vec4 brickSample = texture(sampler2D(SurfaceTexture, SurfaceSampler), input_.TexCoords);
    vec4 treeSample = texture(sampler2D(TreeTexture, TreeSampler), input_.TexCoords);
    brickSample.w = 0.8f;
    vec4 bg = MultiTexturedCube_Shaders_MultiTexturedCubeShader_Over(brickSample, input_.Color);
    return MultiTexturedCube_Shaders_MultiTexturedCubeShader_Over(treeSample, bg);
}


layout(location = 0) in vec2 fsin_0;
layout(location = 1) in vec4 fsin_1;
layout(location = 0) out vec4 _outputColor_;

void main()
{
    MultiTexturedCube_Shaders_MultiTexturedCubeShader_FragmentInput input_;
    input_.SystemPosition = gl_FragCoord;
    input_.TexCoords = fsin_0;
    input_.Color = fsin_1;
    vec4 output_ = FS(input_);
    _outputColor_ = output_;
}
