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

using System.Collections.Generic;
using System.Numerics;
using System.Drawing;
using Veldrid.SceneGraph.Shaders.Standard;
using Veldrid.SceneGraph.Util.Shape;
using Veldrid.SceneGraph.VertexTypes;

namespace Veldrid.SceneGraph.Manipulators
{
    public abstract class Base1DDragger : Dragger
    {
        protected ILineProjector LineProjector { get; set; }

        protected Base1DDragger(Matrix4x4 matrix) : base(matrix)
        {
            LineProjector = Veldrid.SceneGraph.Manipulators.LineProjector.Create(
                LineSegment.Create(new Vector3(-0.5f, 0.0f, 0.0f), new Vector3(0.5f, 0.0f, 0.0f)));
            
            Color = Color.Green;
            PickColor = Color.Magenta;
        }
        
        
    }
}