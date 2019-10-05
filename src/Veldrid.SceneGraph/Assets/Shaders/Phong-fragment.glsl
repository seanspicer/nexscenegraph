#version 450

struct LightData {

    // Color Products
    vec4 AmbientColor;
    vec4 DiffuseColor;
    vec4 SpecularColor;
    
    // Values
    vec4 LightVec;      // Camera Space
    vec4 EyeDirection;  // Camera Space
    vec4 Constants;     // [0] = Power; [1] = SpecularPower; [2] = AttenuationConstant

};

layout(location = 0) in vec3 fsin_normal;
layout(location = 1) in vec3 fsin_eyeDir;
layout(location = 2) flat in LightData fsin_lightData; 

layout(location = 0) out vec4 fsout_color;

void main()
{
    // Inputs
    vec3 n = normalize(fsin_normal);
    vec3 l = normalize(fsin_lightData.LightVec.xyz);
    
    vec3 AmbientColor = fsin_lightData.AmbientColor.xyz;
    vec3 DiffuseColor = fsin_lightData.DiffuseColor.xyz;
    vec3 SpecularColor = fsin_lightData.SpecularColor.xyz;

    float LightPower = fsin_lightData.Constants.x;
    float SpecularPower = fsin_lightData.Constants.y;
    float AttenuationConstant = fsin_lightData.Constants.z;
    
    // Compute the Light Power and Attenuation
    vec3 LightPowerVec = vec3(LightPower, LightPower, LightPower);
    float distance = distance(vec3(0,0,0), fsin_lightData.LightVec.xyz);
    float oneOverDistanceAtten = 1.0f/(pow(distance, AttenuationConstant));
    vec3 Attenuation = vec3(oneOverDistanceAtten, oneOverDistanceAtten, oneOverDistanceAtten);

    // Compute the Diffuse Shading Modifiers
    float cosTheta = clamp( dot( n,l ), 0,1 );
    vec3 CosThetaVec = vec3(cosTheta, cosTheta, cosTheta);
    
    // Eye vector (towards the camera)
    vec3 E = normalize(fsin_eyeDir);
    
    // Direction in which the triangle reflects the light
    vec3 R = reflect(-l,n);
    
    // Cosine of the angle between the Eye vector and the Reflect vector
    float cosAlpha = clamp( dot( E,R ), 0,1 );
    
    // Compute the specular width
    float powCosAlpha = pow(cosAlpha, SpecularPower);
    vec3 SpecularWidthVec = vec3(powCosAlpha,powCosAlpha,powCosAlpha);
    
    vec3 color = AmbientColor + 
                 DiffuseColor * LightPowerVec * CosThetaVec * Attenuation +
                 SpecularColor * LightPowerVec * SpecularWidthVec * Attenuation;
    
    fsout_color = vec4(color, 1.0f);

}
