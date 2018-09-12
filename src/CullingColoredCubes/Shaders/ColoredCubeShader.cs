using System.Numerics;
using ShaderGen;

[assembly: ShaderSet("ColoredCubeShader", "ColoredCube.Shaders.ColoredCubeShader.VS", "ColoredCube.Shaders.ColoredCubeShader.FS")]
namespace ColoredCube.Shaders
{
    public class ColoredCubeShader
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
        public Matrix4x4 Model;

        [VertexShader]
        public FragmentInput VS(ColoredCube.VertexPositionColor input)
        {
            FragmentInput output;
            output.Color = input.Color;
            
            output.Position = Vector4.Transform(
                Vector4.Transform(
                    Vector4.Transform(
                        new Vector4(input.Position, 1f),
                        Model),
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