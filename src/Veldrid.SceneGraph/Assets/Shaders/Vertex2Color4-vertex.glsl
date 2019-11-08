#version 450

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

layout(location = 0) in vec2 Position;
layout(location = 1) in vec4 Color;
layout(location = 0) out vec4 fsin_0;

void main()
{
    fsin_0 = Color;
    //gl_Position = field_Projection * field_View * field_Model * vec4(Position.x, Position.y, 0.0f, 1.f);
    gl_Position = field_Model * vec4(Position.x, Position.y, 0.0f, 1.f);
}
