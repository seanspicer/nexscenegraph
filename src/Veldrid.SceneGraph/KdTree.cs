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

using System;
using System.Numerics;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.Util.Shape;

namespace Veldrid.SceneGraph
{
    public interface IKdTree : IShape
    {
        INode GetNode(int nodeNum);

        internal void Intersect(IIntersectFunctor functor, INode node);

        public interface INode
        {
        }
    }

    public class KdTree : IKdTree
    {
        public void Accept(IShapeVisitor shapeVisitor)
        {
            throw new NotImplementedException();
        }

        public Vector3 Center { get; set; }
        public Quaternion Rotation { get; set; }

        public IKdTree.INode GetNode(int nodeNum)
        {
            throw new NotImplementedException();
        }

        void IKdTree.Intersect(IIntersectFunctor functor, IKdTree.INode node)
        {
            throw new NotImplementedException();
        }
    }
}