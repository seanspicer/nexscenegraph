using System.Drawing;
using ShaderGen;
using System.Numerics;

[assembly: ShaderSet("HelloShaders", "HelloNsg.Shaders.HelloShaders.VS", "HelloNsg.Shaders.HelloShaders.FS")]
namespace HelloNsg.Shaders
{
    public partial class HelloShaders
    {
        public struct FragmentInput
        {
            [SystemPositionSemantic]
            public Vector4 Position;
            [ColorSemantic]
            public Vector4 Color;
        }
        
        public Matrix4x4 Projection;
        public Matrix4x4 View;

        [VertexShader]
        public FragmentInput VS(HelloNsg.VertexPositionColor input)
        {
            FragmentInput output;
            //output.Position = new Vector4(input.Position.X, input.Position.Y, 0, 1);
            output.Color = input.Color;

            output.Position = Vector4.Transform(
                Vector4.Transform(
                    new Vector4(input.Position.X, input.Position.Y, 0, 1),
                    View),
                Projection);
            
            return output;
        }

        [FragmentShader]
        public Vector4 FS(FragmentInput input)
        {
            return input.Color;
        }
    }
}