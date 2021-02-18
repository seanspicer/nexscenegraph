#version 450

#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable

struct FragmentInput
{
    vec4 Position;
    vec3 TexCoord;
    vec4 Color;
};

vec4 CalculateColor(FragmentInput input_) 
{
    vec3 p1 = vec3(0.3, 0.5, 0.3);
    vec3 p2 = vec3(0.7, 0.5, 0.3);
    vec3 p3 = vec3(0.5, 0.5, 0.7);
    
    float d1 = abs(length(input_.TexCoord - p1));
    float d2 = abs(length(input_.TexCoord - p2));
    float d3 = abs(length(input_.TexCoord - p3));
    
    float alpha = 0.1;
    vec4 color = vec4(input_.Color[0], input_.Color[1], input_.Color[2], 0.01);
    
    if(d1 < 0.3 || d2 < 0.3 || d3 < 0.3) {
    
        //alpha = 0.01;
        color = vec4(0.5, 0.0, 0.0, alpha);
    
        //double a1 = 0.2*(0.3-d1);
        //double a2 = 0.2*(0.3-d2);
        //double a3 = 0.2*(0.3-d3);
        //double t = max(a1, a2);//-(d1/0.2);
        //double alpha = max(t, a3);
    }
    
    if(d1 < 0.25 || d2 < 0.25 || d3 < 0.25) {
    
        color = vec4(1, 0.0, 0.0, alpha);
        //alpha = 0.95;//-(d1/0.2);
    }
    
    //vec4 color = vec4(input_.TexCoord[0], input_.TexCoord[1], input_.TexCoord[2], alpha);

    return color; //input_.Color;
}

layout(location = 0) in vec4 fsin_0;
layout(location = 1) in vec3 fsin_1;
layout(location = 0) out vec4 OutputColor;

void main()
{
    FragmentInput input_;
    input_.Position = gl_FragCoord;
    input_.Color = fsin_0;
    input_.TexCoord = fsin_1;
    vec4 output_ = CalculateColor(input_);
    OutputColor = output_;
}