//
// This file is part of IMAGEFrac (R) and related technologies.
//
// Copyright (c) 2017-2020 Reveal Energy Services.  All Rights Reserved.
//
// LEGAL NOTICE:
// IMAGEFrac contains trade secrets and otherwise confidential information
// owned by Reveal Energy Services. Access to and use of this information is 
// strictly limited and controlled by the Company. This file may not be copied,
// distributed, or otherwise disclosed outside of the Company's facilities 
// except under appropriate precautions to maintain the confidentiality hereof, 
// and may not be used in any way not expressly authorized by the Company.
//

using System;

namespace Veldrid.SceneGraph.Shaders.Standard
{
    public class Position3TexCoord3Color4Shader : StandardShaderBase
    {
        private static readonly Lazy<Position3TexCoord3Color4Shader> Lazy = new Lazy<Position3TexCoord3Color4Shader>(() => new Position3TexCoord3Color4Shader());

        public static Position3TexCoord3Color4Shader Instance => Lazy.Value;

        private Position3TexCoord3Color4Shader() 
            : base(@"Position3TexCoord3Color4Shader",@"Position3TexCoord3Color4-vertex.glsl", @"Position3TexCoord3Color4-fragment.glsl")
        {
        }
    }
}