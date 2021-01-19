using System.Collections.Generic;
using System.Numerics;
using Veldrid.SceneGraph.Shaders.Standard;
using Veldrid.SceneGraph.VertexTypes;

namespace Veldrid.SceneGraph.NodeKits.DirectVolumeRendering
{
    public interface ISampledVolumeNode : IGeode
    {
        
    }
    
    public class SampledVolumeNode : Geode, ISampledVolumeNode
    {
        public new static ISampledVolumeNode Create()
        {
            return new SampledVolumeNode();
        }

        protected SampledVolumeNode()
        {
            CreateCube();
        }

        public override void Accept(INodeVisitor visitor)
        {
            visitor.Apply(this);
        }

        private void CreateCube()
        {
            Vector3[] cubeVertices =
            {
                new Vector3(1.0f, 1.0f, -1.0f), // (0) Back top right  
                new Vector3(-1.0f, 1.0f, -1.0f), // (1) Back top left
                new Vector3(1.0f, 1.0f, 1.0f), // (2) Front top right
                new Vector3(-1.0f, 1.0f, 1.0f), // (3) Front top left
                new Vector3(1.0f, -1.0f, -1.0f), // (4) Back bottom right
                new Vector3(-1.0f, -1.0f, -1.0f), // (5) Back bottom left
                new Vector3(1.0f, -1.0f, 1.0f), // (6) Front bottom right
                new Vector3(-1.0f, -1.0f, 1.0f) // (7) Front bottom left
            };

            var cubeTriangleVertices = new List<Position3Color3>();
            var cubeTriangleIndices = new List<uint>();

            uint[] indicies =
            {
                0, 1, 3, 2, 0,
                4, 5, 7, 6, 4,
                0, 2, 6, 4, 0,
                1, 3, 7, 5, 1,
                2, 3, 7, 6, 2,
                0, 1, 5, 4, 0
            };

            Vector3[] normals =
            {
                Vector3.UnitY,
                -Vector3.UnitY,
                Vector3.UnitX,
                -Vector3.UnitX,
                Vector3.UnitZ,
                -Vector3.UnitZ,
            };

            foreach (var t in cubeVertices)
            {
                cubeTriangleVertices.Add(new Position3Color3(
                    t, Vector3.UnitX));
            }

            var vertexData = cubeTriangleVertices.ToArray();

            for (uint i = 0; i < 6; ++i)
            {
                var geometry = Geometry<Position3Color3>.Create();

                geometry.VertexData = vertexData;

                geometry.IndexData = indicies;

                geometry.VertexLayouts = new List<VertexLayoutDescription>()
                {
                    Position3Color3.VertexLayoutDescription
                };

                var pSet = DrawElements<Position3Color3>.Create(
                    geometry,
                    PrimitiveTopology.LineStrip,
                    5,
                    1,
                    5u * i,
                    0,
                    0);

                geometry.PrimitiveSets.Add(pSet);

                geometry.PipelineState.ShaderSet = Position3Color3Shader.Instance.ShaderSet;

                // Set a cull callback that will cull based on the plane normal.
                //geometry.SetCullCallback(new PlaneCullCalback(normals[i]));

                this.AddDrawable(geometry);

            }
        }
    }
}