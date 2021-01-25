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

using System.Numerics;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.Manipulators;
using Veldrid.SceneGraph.NodeKits.DirectVolumeRendering;

namespace Examples.Common
{
    public class DraggersExampleScene
    {
        public static IGroup Build()
        {
            var root = Group.Create();
            
            var scale1DDragger = Scale1DDragger.Create();
            scale1DDragger.SetupDefaultGeometry();
            var scale1DDraggerXForm = MatrixTransform.Create(Matrix4x4.CreateTranslation(-1f, 0.0f, 0.0f));
            scale1DDraggerXForm.AddChild(scale1DDragger);
            root.AddChild(scale1DDraggerXForm);
            
            var scale2DDragger = Scale2DDragger.Create();
            scale2DDragger.SetupDefaultGeometry();
            var scale2DDraggerXForm = MatrixTransform.Create(Matrix4x4.CreateTranslation(1f, 0.0f, 0.0f));
            scale2DDraggerXForm.AddChild(scale2DDragger);
            root.AddChild(scale2DDraggerXForm);
            
            return root;
        }
    }
}