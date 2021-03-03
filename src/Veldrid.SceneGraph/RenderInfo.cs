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

namespace Veldrid.SceneGraph
{
    public class RenderInfo
    {
        public RenderInfo()
        {
        }

        public RenderInfo(View view)
        {
            View = view;
        }

        public RenderInfo(RenderInfo renderInfo)
        {
            View = renderInfo.View;
        }

        public View View { get; set; }

        public GraphicsDevice GraphicsDevice { get; set; }
        public ResourceFactory ResourceFactory { get; set; }
        public CommandList CommandList { get; set; }
        public ResourceLayout ResourceLayout { get; set; }
        public ResourceSet ResourceSet { get; set; }

        // TODO - these dont really belong here.
        public DeviceBuffer VertexBuffer { get; set; }
        public DeviceBuffer IndexBuffer { get; set; }
    }
}