#version 450
#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable

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

layout(location = 0) in vec3 Position;
layout(location = 1) in vec2 TexCoords;
layout(location = 0) out vec2 fsin_0;

void main()
{   
    vec4 v4Pos = vec4(Position, 1);
    gl_Position = field_Projection * field_View * (field_Model * v4Pos);
    fsin_0 = TexCoords;
}