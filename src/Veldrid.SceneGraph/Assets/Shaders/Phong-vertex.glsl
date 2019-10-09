#version 450

struct LightSourceStruct {

    vec3 AmbientColor;
    float LightPower;
    vec3 DiffuseColor;
    float AttenuationConstant;
    vec3 SpecularColor;
    float IsHeadlight;
    vec4 Position;
};

struct MaterialDescStruct {

    vec3 AmbientColor;
    float Shininess;
    vec3 DiffuseColor;
    float Padding0;
    vec3 SpecularColor;
    float MaterialOverride;
    vec4 Padding1;
};

struct LightData {

    // Color Products
    vec4 AmbientColor;
    vec4 DiffuseColor;
    vec4 SpecularColor;
    
    // Values
    vec4 Constants;     // [0] = Power; [1] = SpecularPower; [2] = AttenuationConstant

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

layout(set = 1, binding = 1) uniform LightSource
{
    LightSourceStruct lightSource;
};

layout(set = 1, binding = 2) uniform MaterialDescription
{
    MaterialDescStruct materialDesc;
};

layout(location = 0) in vec3 Position;
layout(location = 1) in vec2 UV;
layout(location = 2) in vec3 Color;
layout(location = 3) in vec3 Normal;

layout(location = 0) out vec3 fsin_normal;
layout(location = 1) out vec3 fsin_eyeDir;
layout(location = 2) out vec3 fsin_lightVec;
layout(location = 3) out LightData fsin_lightData; 

void main()
{
    vec4 LightPos = vec4(0,0,0,1);
    vec4 v4Pos = vec4(Position, 1);
    
    gl_Position = field_Projection * field_View * (field_Model * v4Pos);
    
    vec3 vertexPosition_cameraspace = ( field_View * field_Model * v4Pos).xyz;
    vec3 EyeDirection_cameraspace = vec3(0,0,0) - vertexPosition_cameraspace;
    
    vec3 LightPosition_cameraspace = (1-lightSource.IsHeadlight) * ( field_View * field_Model * lightSource.Position).xyz;
    vec3 LightDirection_cameraspace = LightPosition_cameraspace + EyeDirection_cameraspace;
    
    vec3 Normal_cameraspace = ( field_View * transpose(inverse(field_Model)) * vec4(Normal,0)).xyz;
    
    fsin_eyeDir = EyeDirection_cameraspace;
    fsin_normal = Normal_cameraspace;
    fsin_lightVec = LightDirection_cameraspace;
    
    float A = materialDesc.MaterialOverride;
    float B = 1-A;
    
    fsin_lightData.AmbientColor = vec4(A*(materialDesc.AmbientColor) + B*(Color*lightSource.AmbientColor), 1.0f);
    fsin_lightData.DiffuseColor = vec4(A*(materialDesc.DiffuseColor) + B*(Color*lightSource.DiffuseColor), 1.0f);
    fsin_lightData.SpecularColor = vec4(materialDesc.SpecularColor, 1.0f);
    
    fsin_lightData.Constants = vec4(lightSource.LightPower, materialDesc.Shininess, lightSource.AttenuationConstant, 1.0f);
}
