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
            public Vector2 Position;
            [ColorTargetSemantic]
            public Vector4 Color;
        }

        [VertexShader]
        public FragmentInput VS(HelloNsg.VertexPositionColor input)
        {
            FragmentInput output;
            output.Position = input.Position;
            output.Position.Y = -output.Position.Y;
            output.Color = input.Color;

            return output;
        }

        [FragmentShader]
        public Vector4 FS(FragmentInput input)
        {
            return input.Color;
        }
    }
}