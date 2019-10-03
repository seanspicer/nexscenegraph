#version 450

struct LightSourceStruct {

    vec3 AmbientColor;
    float LightPower;
    vec3 DiffuseColor;
    float AttenuationConstant;
    vec3 SpecularColor;
    int IsHeadlight;
    vec4 Position;
};

struct MaterialDescStruct {

    vec3 AmbientColor;
    float Shininess;
    vec3 DiffuseColor;
    float Padding0;
    vec3 SpecularColor;
    int MaterialOverride;
    vec3 Padding2;
};

layout(set = 1, binding = 1) uniform LightSource
{
    LightSourceStruct lightSource;
};

layout(set = 1, binding = 2) uniform MaterialDescription
{
    MaterialDescStruct materialDesc;
};

layout(location = 0) in vec3 fsin_normal;
layout(location = 1) in vec3 fsin_color;
layout(location = 2) in vec3 fsin_eyePos;
layout(location = 3) in vec3 fsin_lightVec;

layout(location = 0) out vec4 fsout_color;

void main()
{
    // Inputs
    vec3 n = normalize(fsin_normal);
    vec3 l = normalize(fsin_lightVec);
    vec3 e = normalize(fsin_eyePos);
    
    vec3 MaterialAmbientColor = materialDesc.AmbientColor;
    vec3 MaterialDiffuseColor = materialDesc.DiffuseColor;
    vec3 MaterialSpecularColor = materialDesc.SpecularColor;
    
    if(0 == materialDesc.MaterialOverride) {
       MaterialAmbientColor = fsin_color;
       MaterialDiffuseColor = fsin_color;
    } 

    float LightPower = lightSource.LightPower;
    float SpecularPower = materialDesc.Shininess;
    
    // Compute the Light Power and Attenuation
    vec3 LightPowerVec = vec3(LightPower, LightPower, LightPower);
    float distance = distance(vec3(0,0,0), fsin_lightVec);
    float oneOverDistanceAtten = 1.0f/(distance);//(pow(distance, lightSource.AttenuationConstant));
    vec3 Attenuation = vec3(oneOverDistanceAtten, oneOverDistanceAtten, oneOverDistanceAtten);

    // Compute the Diffuse Shading Modifiers
    float cosTheta = clamp( dot( n,l ), 0,1 );
    vec3 CosThetaVec = vec3(cosTheta, cosTheta, cosTheta);
    
    // Eye vector (towards the camera)
    vec3 E = l;
    
    // Direction in which the triangle reflects the light
    vec3 R = reflect(-l,n);
    
    // Cosine of the angle between the Eye vector and the Reflect vector
    float cosAlpha = clamp( dot( E,R ), 0,1 );
    
    // Compute the specular width
    float powCosAlpha = pow(cosAlpha, SpecularPower);
    vec3 SpecularWidthVec = vec3(powCosAlpha,powCosAlpha,powCosAlpha);
    
    vec3 color = MaterialAmbientColor * lightSource.AmbientColor + 
                 MaterialDiffuseColor * lightSource.DiffuseColor * LightPowerVec * CosThetaVec * Attenuation + 
                 MaterialSpecularColor * lightSource.SpecularColor * LightPowerVec * SpecularWidthVec * Attenuation;
    
    fsout_color = vec4(color, 1.0f);

}
