#version 450

layout(location = 0) in vec3 fsin_normal;
layout(location = 1) in vec3 fsin_color;
layout(location = 2) in vec3 fsin_eyePos;
layout(location = 3) in vec3 fsin_lightVec;
layout(location = 4) in vec3 fsin_light_color;
layout(location = 5) in float fsin_light_power;
layout(location = 6) in vec3 fsin_specular_color;
layout(location = 7) in float fsin_specular_power;

layout(location = 0) out vec4 fsout_color;

void main()
{
    // Inputs
    vec3 n = normalize(fsin_normal);
    vec3 l = normalize(fsin_lightVec);
    vec3 e = normalize(fsin_eyePos);

    vec3 LightColor = fsin_light_color;
    vec3 MaterialSpecularColor = fsin_specular_color;
    float LightPower = fsin_light_power;
    float SpecularPower = fsin_specular_power;
    
    vec3 MaterialDiffuseColor = fsin_color;
    vec3 MaterialAmbientColor = vec3(0.1,0.1,0.1) * MaterialDiffuseColor;
    
    // Compute the Light Power and Attenuation
    vec3 LightPowerVec = vec3(LightPower, LightPower, LightPower);
    float distance = distance(vec3(0,0,0), fsin_lightVec);
    float oneOverDistanceSquared = 1.0f/(distance);
    vec3 Attenuation = vec3(oneOverDistanceSquared, oneOverDistanceSquared, oneOverDistanceSquared);

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
    
    vec3 color = MaterialAmbientColor +
                 MaterialDiffuseColor * LightColor * LightPowerVec * CosThetaVec * Attenuation + 
                 MaterialSpecularColor * LightColor * LightPowerVec * SpecularWidthVec * Attenuation;
    
    fsout_color = vec4(color, 1.0f);

}
