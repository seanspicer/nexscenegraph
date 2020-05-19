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
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.PipelineStates;
using Veldrid.SceneGraph.Util.Shape;
using Veldrid.SceneGraph.VertexTypes;
using Path = Veldrid.SceneGraph.Util.Shape.Path;

namespace Examples.Common
{
    public class PathExampleScene
    {
        public static IGroup Build()
        {
            var root = Group.Create();

            var path = Path.Create(new Vector3[]
            {
                new Vector3(0.0f,  1.0f, 0.0f),
                new Vector3(0.0f,  0.0f, 0.0f), 
                new Vector3(1.0f,  -1.0f, 0.0f),
                new Vector3(2.0f, -2.0f, 2.0f), 
            });

            var hints = TessellationHints.Create();
            hints.SetDetailRatio(4f);
            hints.SetRadius(.1f);
            var pathDrawable = ShapeDrawable<Position3Texture2Color3Normal3>.Create(path, hints);
            
            var redMaterial = PhongMaterial.Create(
                PhongMaterialParameters.Create(
                    new Vector3(1.0f, 0.0f, 0.0f),
                    new Vector3(1.0f, 0.0f, 0.0f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    5f),
                PhongHeadlight.Create(PhongLightParameters.Create(
                    new Vector3(0.1f, 0.1f, 0.1f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    1f,
                    0)),
                true);

            var pathGeode = Geode.Create();
            pathGeode.AddDrawable(pathDrawable);
            pathGeode.PipelineState = redMaterial.CreatePipelineState();
            pathGeode.PipelineState.RasterizerStateDescription 
                = new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, false);

            root.AddChild(pathGeode);
            return root;
        }
    }
}