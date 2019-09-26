#version 450

/*
layout(set = 0, binding = 0) uniform UBO
{
    mat4 Projection;
    mat4 View;
    mat4 Model;
    vec4 LightPos;
};*/

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
layout(location = 1) in vec2 UV;
layout(location = 2) in vec3 Color;
layout(location = 3) in vec3 Normal;
layout(location = 0) out vec3 fsin_normal;
layout(location = 1) out vec3 fsin_color;
layout(location = 2) out vec3 fsin_eyePos;
layout(location = 3) out vec3 fsin_lightVec;

void main()
{
    vec4 LightPos = vec4(0,0,0,1);
    vec4 v4Pos = vec4(Position, 1);
    fsin_normal = Normal;
    fsin_color = Color;
    gl_Position = field_Projection * field_View * (field_Model * v4Pos);
    vec4 eyePos = field_View * field_Model * v4Pos;
    fsin_eyePos = eyePos.xyz;
    vec4 eyeLightPos = field_View * LightPos;
    fsin_lightVec = normalize(LightPos.xyz - fsin_eyePos);
}
