using System;

namespace Veldrid.SceneGraph.Shaders.Standard
{
    public class Position3TexCoord3Color4Shader : StandardShaderBase
    {
        private static readonly Lazy<Position3TexCoord3Color4Shader> Lazy =
            new Lazy<Position3TexCoord3Color4Shader>(() => new Position3TexCoord3Color4Shader());

        private Position3TexCoord3Color4Shader()
            : base(@"Position3TexCoord3Color4Shader", @"Position3TexCoord3Color4-vertex.glsl",
                @"Position3TexCoord3Color4-fragment.glsl")
        {
        }

        public static Position3TexCoord3Color4Shader Instance => Lazy.Value;
    }
}