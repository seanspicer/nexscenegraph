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
using System.Linq;
using System.Numerics;
using Veldrid.SceneGraph.InputAdapter;


namespace Veldrid.SceneGraph.Manipulators
{
    public interface ICompositeDragger : IDragger
    {
    }

    public class CompositeDragger : Dragger, ICompositeDragger
    {
        protected CompositeDragger(Matrix4x4 matrix) : base(matrix)
        {
        }

        protected List<IDragger> DraggerList { get; set; } = new List<IDragger>();

        public override IDragger ParentDragger
        {
            get => base.ParentDragger;
            set
            {
                foreach (var dragger in DraggerList) dragger.ParentDragger = value;

                base.ParentDragger = value;
            }
        }

        public override uint IntersectionNodeMask
        {
            get => base.IntersectionNodeMask;
            set
            {
                foreach (var dragger in DraggerList) dragger.IntersectionNodeMask = value;

                base.IntersectionNodeMask = value;
            }
        }

        public override bool Handle(IPointerInfo pointerInfo, IUiEventAdapter eventAdapter,
            IUiActionAdapter actionAdapter)
        {
            if (!pointerInfo.Contains(this)) return false;

            return DraggerList.Any(dragger => dragger.Handle(pointerInfo, eventAdapter, actionAdapter));
        }
    }
}