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

using System.Drawing;
using System.Text;

namespace Veldrid.SceneGraph.Util.Shape
{
    public enum NormalsType
    {
        PerFace,
        PerVertex
    }

    public enum ColorsType
    {
        ColorOverall,
        ColorPerFace,
        ColorPerVertex,
    }
    
    public interface ITessellationHints
    {
        NormalsType NormalsType { get; set; }
        ColorsType ColorsType { get; set; }
    }
    
    public class TessellationHints : ITessellationHints
    {
        public NormalsType NormalsType { get; set; }
        public ColorsType ColorsType { get; set; }

        public static ITessellationHints Create()
        {
            return new TessellationHints();
        }

        internal TessellationHints()
        {
            NormalsType = NormalsType.PerVertex;
            ColorsType = ColorsType.ColorOverall;
        }
    }
}