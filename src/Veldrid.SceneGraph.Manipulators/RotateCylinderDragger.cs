//
// Copyright 2018-2021 Sean Spicer 
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System.Collections.Generic;
using System.Numerics;
using Veldrid.SceneGraph.PipelineStates;
using Veldrid.SceneGraph.Util.Shape;
using Veldrid.SceneGraph.VertexTypes;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface IRotateCylinderDragger : IDragger
    {
    }

    public class RotateCylinderDragger : Dragger, IRotateCylinderDragger
    {
        private readonly IPhongMaterial _material;
        private IPhongMaterial _pickedMaterial;
        protected Quaternion _prevRotation;

        protected Vector3 _prevWorldProjPt;
        protected ICylinderPlaneProjector _projector;
        protected Matrix4x4 _startLocalToWorld;
        protected Matrix4x4 _startWorldToLocal;

        protected RotateCylinderDragger(Matrix4x4 matrix, bool usePhongShading = true) : base(matrix, usePhongShading)
        {
            _projector = CylinderPlaneProjector.Create();
            _material = CreateMaterial();
            _pickedMaterial = CreateMaterial();
        }

        public override void SetupDefaultGeometry()
        {
            var geode = Geode.Create();
            {
                var hints = TessellationHints.Create();
                hints.CreateTop = false;
                hints.CreateBottom = false;
                hints.CreateBackFace = false;
                hints.SetDetailRatio(4);

                var radius = 1.0f;
                var height = 0.1f;
                var thickness = 0.1f;

                var cylinder = Cylinder.Create(Vector3.Zero, radius, height);
                var cylinderDrawable = ShapeDrawable<Position3Texture2Color3Normal3>.Create(cylinder, hints);
                cylinderDrawable.NameString = "Outer Cylinder";
                geode.AddDrawable(cylinderDrawable);


                var innerCylinder = Cylinder.Create(_projector.Cylinder.Center, radius - thickness, height);
                var innerCylinderDrawable = ShapeDrawable<Position3Texture2Color3Normal3>.Create(innerCylinder, hints);
                innerCylinderDrawable.NameString = "Inner Cylinder";
                geode.AddDrawable(innerCylinderDrawable);

                // Top
                var topGeom = CreateDiskGeometry(radius, thickness, height / 2, Vector3.UnitZ, 100);
                geode.AddDrawable(topGeom);

                // Bottom
                var bottomGeom = CreateDiskGeometry(radius, thickness, -height / 2, -Vector3.UnitZ, 100);
                geode.AddDrawable(bottomGeom);
            }

            var pso = _material.CreatePipelineState();
            geode.PipelineState = pso;

            AddChild(geode);
        }

        public new static IRotateCylinderDragger Create()
        {
            return new RotateCylinderDragger(Matrix4x4.Identity);
        }

        public IGeometry<Position3Texture2Color3Normal3> CreateDiskGeometry(float radius, float offset, float z,
            Vector3 normal, uint numSegments)
        {
            var angleDelta = 2.0f * (float) System.Math.PI / numSegments;
            var numPoints = (numSegments + 1) * 2;
            var angle = 0.0f;

            var vertexArray = new Position3Texture2Color3Normal3[numPoints];
            var indexArray = new uint[numPoints];
            var p = 0u;
            for (var i = 0; i < numSegments; ++i, angle += angleDelta)
            {
                var c = System.Math.Cos(angle);
                var s = System.Math.Sin(angle);
                // Outer point
                vertexArray[p] = new Position3Texture2Color3Normal3(
                    new Vector3((float) (radius * c), (float) (radius * s), z),
                    Vector2.Zero, Vector3.One,
                    normal);
                indexArray[p] = p;
                ++p;
                // Inner point
                vertexArray[p] = new Position3Texture2Color3Normal3(
                    new Vector3((float) ((radius - offset) * c), (float) ((radius - offset) * s), z),
                    Vector2.Zero, Vector3.One, normal);
                indexArray[p] = p;
                ++p;
            }

            // do last points by hand to ensure no round off errors.
            vertexArray[p] = vertexArray[0];
            indexArray[p] = p;
            ++p;
            vertexArray[p] = vertexArray[1];
            indexArray[p] = p;

            var geometry = Geometry<Position3Texture2Color3Normal3>.Create();
            geometry.IndexData = indexArray;
            geometry.VertexData = vertexArray;
            geometry.VertexLayouts = new List<VertexLayoutDescription>
            {
                Position3Texture2Color3Normal3.VertexLayoutDescription
            };

            var pSet = DrawElements<Position3Texture2Color3Normal3>.Create(
                geometry,
                PrimitiveTopology.TriangleStrip,
                (uint) indexArray.Length,
                1,
                0,
                0,
                0);

            geometry.PrimitiveSets.Add(pSet);

            return geometry;
        }
    }
}