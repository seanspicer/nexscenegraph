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
using Veldrid.SceneGraph.Manipulators.Commands;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface IConstraint
    {
        bool Constrain(IMotionCommand motionCommand);
        bool Constrain(ITranslateInLineCommand command);
        bool Constrain(ITranslateInPlaneCommand command);
        bool Constrain(IScale1DCommand command);
        bool Constrain(IScale2DCommand command);
        bool Constrain(IScaleUniformCommand command);
        bool Constrain(IRotate3DCommand command);
    }

    public class Constraint : IConstraint
    {
        private readonly Matrix4x4 _localToWorld = Matrix4x4.Identity;
        private readonly Matrix4x4 _worldToLocal = Matrix4x4.Identity;

        protected INode ReferenceNode { get; set; }

        public virtual bool Constrain(IMotionCommand command)
        {
            return false;
        }

        public virtual bool Constrain(ITranslateInLineCommand command)
        {
            return Constrain(command as IMotionCommand);
        }

        public virtual bool Constrain(ITranslateInPlaneCommand command)
        {
            return Constrain(command as IMotionCommand);
        }

        public virtual bool Constrain(IScale1DCommand command)
        {
            return Constrain(command as IMotionCommand);
        }

        public virtual bool Constrain(IScale2DCommand command)
        {
            return Constrain(command as IMotionCommand);
        }

        public virtual bool Constrain(IScaleUniformCommand command)
        {
            return Constrain(command as IMotionCommand);
        }

        public virtual bool Constrain(IRotate3DCommand command)
        {
            return Constrain(command as IMotionCommand);
        }

        protected Matrix4x4 GetLocalToWorld()
        {
            return _localToWorld;
        }

        protected Matrix4x4 GetWorldToLocal()
        {
            return _worldToLocal;
        }

        protected void ComputeLocalToWorldAndWorldToLocal()
        {
        }
    }
}