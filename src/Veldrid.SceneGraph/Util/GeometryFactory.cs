//
// Copyright 2018-2019 Sean Spicer 
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

using System;

namespace Veldrid.SceneGraph.Util
{
    public enum VertexType
    {
        Position3Texture2Color3Normal3
    }

    public enum TopologyType
    {
        IndexedTriangleList
    }

    public interface IGeometryFactory
    {
        IGeode CreateCube(VertexType vertexType, TopologyType topologyType);
    }

    public class GeometryFactory : IGeometryFactory
    {
        internal GeometryFactory()
        {
        }

        public IGeode CreateCube(VertexType vertexType, TopologyType topologyType)
        {
            if (vertexType == VertexType.Position3Texture2Color3Normal3 &&
                topologyType == TopologyType.IndexedTriangleList)
                return CubeGeometry.CreatePosition3Texture2Color3Normal3_IndexedTriangleList();

            throw new ArgumentException("Invalid arguments");
        }

        public static IGeometryFactory Create()
        {
            return new GeometryFactory();
        }
    }
}