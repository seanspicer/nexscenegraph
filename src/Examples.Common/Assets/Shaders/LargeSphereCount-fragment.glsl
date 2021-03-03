//
// Copyright 2018-2021 Sean Spicer 
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

#version 450

struct LightData {

    // Color Products
    vec4 AmbientColor;
    vec4 DiffuseColor;
    vec4 SpecularColor;
    
    // Values
    vec4 Constants;     // [0] = Power; [1] = SpecularPower; [2] = AttenuationConstant

};

layout(location = 0) in vec3 fsin_normal;
layout(location = 1) in vec3 fsin_eyeDir;
layout(location = 2) in vec3 fsin_lightVec;
layout(location = 3) in vec3 fsin_AmbientColor;
layout(location = 4) in vec3 fsin_DiffuseColor;
layout(location = 5) in vec3 fsin_SpecularColor;
layout(location = 6) in vec3 fsin_Constants;
layout(location = 7) in float fsin_VisibilityFlag;

//layout(location = 4) flat in LightData fsin_lightData; 

layout(location = 0) out vec4 fsout_color;

void main()
{
    if(fsin_VisibilityFlag < 0.5) discard;

    // Inputs
    vec3 n = normalize(fsin_normal);
    vec3 l = normalize(fsin_lightVec);
    
    vec3 AmbientColor = fsin_AmbientColor;
    vec3 DiffuseColor = fsin_DiffuseColor;
    vec3 SpecularColor = fsin_SpecularColor;

    float LightPower = fsin_Constants.x;
    float SpecularPower = fsin_Constants.y;
    float AttenuationConstant = fsin_Constants.z;
    
    // Compute the Light Power and Attenuation
    vec3 LightPowerVec = vec3(LightPower, LightPower, LightPower);
    float distance = distance(vec3(0,0,0), fsin_lightVec);
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
