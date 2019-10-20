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
    /// <summary>
    /// Base class for all intersectors
    /// </summary>
    public abstract class Intersector : IIntersector
    {
        public enum IntersectionLimitModes
        {
            NoLimit,
            LimitOnePerDrawable,
            LimitOne,
            LimitNearest
        };
        
        public IntersectionLimitModes IntersectionLimit { get; set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        protected Intersector()
        {
            IntersectionLimit = IntersectionLimitModes.NoLimit;
        }

        public abstract Intersector Clone(IIntersectionVisitor iv);
        
        public abstract void Intersect(IIntersectionVisitor iv, IDrawable drawable);

        public abstract bool Enter(INode node);

        public abstract void Leave();

        public abstract void Reset();

    }
}