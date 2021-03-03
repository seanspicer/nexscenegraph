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
        ColorPerVertex
    }

    public interface ITessellationHints
    {
        NormalsType NormalsType { get; set; }
        ColorsType ColorsType { get; set; }
        bool CreateFrontFace { get; set; }
        bool CreateBackFace { get; set; }
        bool CreateBody { get; set; }
        bool CreateTop { get; set; }
        bool CreateBottom { get; set; }
        bool CreateEndCaps { get; set; }
        float DetailRatio { get; }
        float Radius { get; }
        void SetDetailRatio(float detailRatio);
        void SetRadius(float radius);
    }

    public class TessellationHints : ITessellationHints
    {
        internal TessellationHints()
        {
            CreateFrontFace = true;
            CreateBackFace = true;
            CreateEndCaps = true;
            CreateBody = true;
            CreateTop = true;
            CreateBottom = true;
            NormalsType = NormalsType.PerVertex;
            ColorsType = ColorsType.ColorOverall;
            DetailRatio = 1.0f;
            Radius = 1.0f;
        }

        public bool CreatePathAsLine { get; set; }
        public NormalsType NormalsType { get; set; }
        public ColorsType ColorsType { get; set; }
        public bool CreateFrontFace { get; set; }
        public bool CreateBackFace { get; set; }
        public bool CreateBody { get; set; }
        public bool CreateTop { get; set; }
        public bool CreateBottom { get; set; }
        public bool CreateEndCaps { get; set; }
        public float DetailRatio { get; private set; }
        public float Radius { get; private set; }

        public void SetDetailRatio(float detailRatio)
        {
            if (detailRatio <= 0.0) throw new ArgumentException("Detail Ratio must be greater than 0.0");

            DetailRatio = detailRatio;
        }

        public void SetRadius(float radius)
        {
            if (radius < 0.0) throw new ArgumentException("Radius must be greater than 0.0");

            Radius = radius;
        }

        public static ITessellationHints Create()
        {
            return new TessellationHints();
        }
    }
}