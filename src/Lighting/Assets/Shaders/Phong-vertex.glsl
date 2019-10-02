#version 450

/*
layout(set = 0, binding = 0) uniform UBO
{
    mat4 Projection;
    mat4 View;
    mat4 Model;
    vec4 LightPos;
};*/

struct LightDataStruct {
    vec3 light_color;
    float light_power;
    vec3 specular_color;
    float specular_power;
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

layout(set = 1, binding = 1) uniform LightData
{
    LightDataStruct ld;
};

layout(location = 0) in vec3 Position;
layout(location = 1) in vec2 UV;
layout(location = 2) in vec3 Color;
layout(location = 3) in vec3 Normal;
layout(location = 0) out vec3 fsin_normal;
layout(location = 1) out vec3 fsin_color;
layout(location = 2) out vec3 fsin_eyePos;
layout(location = 3) out vec3 fsin_lightVec;
layout(location = 4) out vec3 fsin_light_color;
layout(location = 5) out float fsin_light_power;
layout(location = 6) out vec3 fsin_specular_color;
layout(location = 7) out float fsin_specular_power;

void main()
{
    vec4 LightPos = vec4(0,0,0,1);
    vec4 v4Pos = vec4(Position, 1);
    fsin_color = Color;
    gl_Position = field_Projection * field_View * (field_Model * v4Pos);
    
    vec3 vertexPosition_cameraspace = ( field_View * field_Model * v4Pos).xyz;
    vec3 EyeDirection_cameraspace = vec3(0,0,0) - vertexPosition_cameraspace;
    
    vec3 LightPosition_cameraspace = vec3(0, 0, 0); //( field_View * field_Model * vec4(0,0,100,1)).xyz;
    vec3 LightDirection_cameraspace = LightPosition_cameraspace + EyeDirection_cameraspace;
    
    vec3 Normal_cameraspace = ( field_View * transpose(inverse(field_Model)) * vec4(Normal,0)).xyz;
    
    fsin_eyePos = vec3(0,0,0);
    fsin_normal = Normal_cameraspace;
    fsin_lightVec = LightDirection_cameraspace;
    fsin_light_color = ld.light_color;
    fsin_light_power = ld.light_power;
    fsin_specular_color = ld.specular_color;
    fsin_specular_power = ld.specular_power;
}
