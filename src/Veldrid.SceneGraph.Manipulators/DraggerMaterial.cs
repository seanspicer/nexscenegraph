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

using System.Numerics;
using Veldrid.SceneGraph.PipelineStates;

namespace Veldrid.SceneGraph.Manipulators
{
    internal class DraggerMaterial : PhongMaterial
    {
        protected DraggerMaterial(IPhongMaterialParameters m, PhongLight l) : base(m, l, true)
        {
        }

        internal static IPhongMaterial Create(bool usePhongShading = true)
        {
            if (usePhongShading)
                return new DraggerMaterial(PhongMaterialParameters.Create(
                        new Vector3(0.0f, 1.0f, 0.0f),
                        new Vector3(0.0f, 1.0f, 0.0f),
                        new Vector3(0.0f, 0.0f, 0.0f),
                        1f),
                    PhongHeadlight.Create(PhongLightParameters.Create(
                        new Vector3(0.2f, 0.2f, 0.2f),
                        new Vector3(0.2f, 0.2f, 0.2f),
                        new Vector3(0.0f, 0.0f, 0.0f),
                        5f,
                        0)));
            return new DraggerMaterial(PhongMaterialParameters.Create(
                    new Vector3(0.0f, 1.0f, 0.0f),
                    new Vector3(0.0f, 1.0f, 0.0f),
                    new Vector3(0.0f, 0.0f, 0.0f),
                    1f),
                PhongHeadlight.Create(PhongLightParameters.Create(
                    new Vector3(1f, 1f, 1f),
                    new Vector3(0f, 0f, 0f),
                    new Vector3(0.0f, 0.0f, 0.0f),
                    5f,
                    0)));
        }

        public override IPipelineState CreatePipelineState()
        {
            var state = base.CreatePipelineState();
            state.RasterizerStateDescription = RasterizerStateDescription.CullNone;
            return state;
        }
    }
}