//
// Copyright 2018 Sean Spicer 
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
using System.Collections.Generic;
using System.Numerics;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.Viewer;

namespace Veldrid.SceneGraph
{

    public class Billboard : Geode, IBillboard
    {
        public enum Modes
        {
            Screen
        }

        public Modes Mode { get; set; }

        public new static IBillboard Create()
        {
            return new Billboard();
        }
        
        protected Billboard()
        {
            Mode = Modes.Screen;
        }
        
        public override void Accept(INodeVisitor visitor)
        {
            visitor.Apply(this);
        }
        
       
        public Matrix4x4 ComputeMatrix(Matrix4x4 modelView, Vector3 eyeLocal)
        {
            Matrix4x4 rotate = Matrix4x4.Identity;
            switch (Mode)
            {
                case Modes.Screen:
                    var tmp = modelView.SetTranslation(Vector3.Zero);
                    var canInvert = Matrix4x4.Invert(tmp, out rotate);
                    if (false == canInvert)
                    {
                        rotate = Matrix4x4.Identity;
                    }
                    break;
                default:
                    break;
            }

            return rotate;
        }
    }
}