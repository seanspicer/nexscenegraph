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

using System.Numerics;

namespace Veldrid.SceneGraph.Manipulators
{
    public abstract class Base1DDragger : Dragger
    {
        protected Base1DDragger(Matrix4x4 matrix, bool usePhongShading) : this(new Vector3(-0.5f, 0.0f, 0.0f),
            new Vector3(0.5f, 0.0f, 0.0f), matrix, usePhongShading)
        {
        }

        protected Base1DDragger(Vector3 s, Vector3 e, Matrix4x4 matrix, bool usePhongShading) : base(matrix,
            usePhongShading)
        {
            LineProjector = Manipulators.LineProjector.Create(
                LineSegment.Create(s, e));
        }

        protected ILineProjector LineProjector { get; set; }
    }
}