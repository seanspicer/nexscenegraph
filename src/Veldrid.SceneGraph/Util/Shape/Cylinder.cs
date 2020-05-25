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

namespace Veldrid.SceneGraph.Util.Shape
{
    public interface ICylinder : IShape
    {
        Vector3 Center { get; }
        float Radius { get; }
        float Height { get; }
    }

    public class Cylinder : ICylinder
    {
        public Vector3 Center { get; }

        public float Radius { get; }

        public float Height { get; }

        public static ICylinder Create(Vector3 center, float radius, float height)
        {
            return new Cylinder(center, radius, height);
        }

        public static ICylinder CreateUnitCone()
        {
            return Create(Vector3.Zero, 0.5f, 1);
        }

        internal Cylinder(Vector3 center, float radius, float height)
        {
            Center = center;
            Radius = radius;
            Height = height;
        }

        public void Accept(IShapeVisitor shapeVisitor)
        {
            shapeVisitor.Apply(this);
        }
    }
}