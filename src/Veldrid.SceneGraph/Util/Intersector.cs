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

namespace Veldrid.SceneGraph.Util
{
    public interface IIntersector
    {
        enum CoordinateFrameMode
        {
            Window,
            Projection,
            View,
            Model
        }

        enum IntersectionLimitModes
        {
            NoLimit,
            LimitOnePerDrawable,
            LimitOne,
            LimitNearest
        }

        enum PrecisionHintTypes
        {
            UseDoubleCalculations,
            UseFloatCalculations
        }

        IntersectionLimitModes IntersectionLimit { get; }
        PrecisionHintTypes PrecisionHint { get; }
        CoordinateFrameMode CoordinateFrame { get; }
        IIntersector Clone(IIntersectionVisitor iv);
        void Intersect(IIntersectionVisitor iv, IDrawable drawable);
        bool Enter(INode node);
        void Leave();
        void Reset();
        bool ReachedLimit();
    }

    /// <summary>
    ///     Base class for all intersectors
    /// </summary>
    public abstract class Intersector : IIntersector
    {
        protected Intersector(IIntersector.CoordinateFrameMode coordinateFrame = IIntersector.CoordinateFrameMode.Model,
            IIntersector.IntersectionLimitModes intersectionLimit = IIntersector.IntersectionLimitModes.NoLimit)
        {
            CoordinateFrame = coordinateFrame;
            IntersectionLimit = intersectionLimit;
        }

        public IIntersector.PrecisionHintTypes PrecisionHint { get; set; } =
            IIntersector.PrecisionHintTypes.UseFloatCalculations;

        public IIntersector.IntersectionLimitModes IntersectionLimit { get; protected set; }

        public IIntersector.CoordinateFrameMode CoordinateFrame { get; protected set; }

        public abstract IIntersector Clone(IIntersectionVisitor iv);

        public abstract void Intersect(IIntersectionVisitor iv, IDrawable drawable);

        public abstract bool Enter(INode node);

        public abstract void Leave();

        public abstract void Reset();

        public bool ReachedLimit()
        {
            return IntersectionLimit == IIntersector.IntersectionLimitModes.LimitOne && ContainsIntersections();
        }

        protected abstract bool ContainsIntersections();
    }
}