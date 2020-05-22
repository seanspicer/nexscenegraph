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
using System.Windows.Media;
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.PipelineStates;
using Veldrid.SceneGraph.Util.Shape;
using Veldrid.SceneGraph.VertexTypes;
using MatrixTransform = Veldrid.SceneGraph.MatrixTransform;

namespace Gnomon
{
    public interface IBaseGnomon : IGeode
    {
        
    }
    
    public class BaseGnomon : Geode, IBaseGnomon
    {
        
        public static IGeode Build()
        {
            var na =  new SimpleGnomon();
            na.Init();
            return na;
        }
    }
    
    public class SimpleGnomon : BaseGnomon
    {
        internal SimpleGnomon()
        {
            Init();
        }

        protected internal void Init()
        {
            setUpSubGraph();
            CreatePipelineState();
        }

        private void setUpSubGraph()
        {
            var len = 10;
            var n = CreateCylinder(new Vector3(0, 0, 0), 
                new Vector3(0, len, 0),
                Color.FromScRgb(1, 0, .8f, 0f));
            var e = CreateCylinder(new Vector3(0, 0, 0), 
                new Vector3(len, 0, 0),
                Color.FromScRgb(1, .8f, 0f, 0f));
            var u = CreateCylinder(new Vector3(0, 0, 0), 
                new Vector3(0, 0, len),
                Color.FromScRgb(1, 0f, 0f, .8f));

            var translate = MatrixTransform.Create();
            translate.AddChild(n);
            translate.AddChild(e);
            translate.AddChild(u);
            
            AddChild(translate);
        }

        private IDrawable CreateCylinder(Vector3 start, Vector3 stop, Color color)
        {
            var path = Path.Create(new []{start, stop});
            var hints = TessellationHints.Create();
            hints.SetDetailRatio(10f);
            hints.SetRadius(1.0f);
            var pathDrawable = ShapeDrawable<Position3Texture2Color3Normal3>.Create(path, hints);

            pathDrawable.PipelineState = CreateMaterial(color).CreatePipelineState();
            
            return pathDrawable;
        }
        
        protected virtual RasterizerStateDescription DefineRasterizerStateDescription() =>
            new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, false);

        protected virtual void CreatePipelineState()
        {
            var material = CreateMaterial(Color.FromScRgb(1, .8f, 0.8f, 0f));
            PipelineState = material.CreatePipelineState();
            PipelineState.RasterizerStateDescription = DefineRasterizerStateDescription();
        }
        
        protected IPhongMaterial CreateMaterial(Color color)
        {
            return PhongMaterial.Create(
                PhongMaterialParameters.Create(
                    color.ToVector3(),
                    color.ToVector3(),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    5f),
                PhongHeadlight.Create(PhongLightParameters.Create(
                    new Vector3(0.3f, 0.3f, 0.3f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    1f,
                    0)));
        }
    }
    
    public static class ColorExtensions
    {
        private const float MaxColor = 255.0f;

        public static Vector3 ToVector3(this Color color) =>
            new Vector3(color.R / MaxColor, color.G / MaxColor, color.B / MaxColor);
    }
}