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
using Veldrid.SceneGraph.Manipulators.Commands;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface IDraggerCallback
    {
        bool Receive(IMotionCommand command);
        bool Receive(ITranslateInLineCommand command);
        bool Receive(ITranslateInPlaneCommand command);
        bool Receive(IScale1DCommand command);
        bool Receive(IScale2DCommand command);
        bool Receive(IScaleUniformCommand command);
        bool Receive(IRotate3DCommand command);
    }

    public class DraggerCallback : IDraggerCallback
    {
        public virtual bool Receive(IMotionCommand command)
        {
            return false;
        }

        public virtual bool Receive(ITranslateInLineCommand command)
        {
            return Receive(command as IMotionCommand);
        }

        public virtual bool Receive(ITranslateInPlaneCommand command)
        {
            return Receive(command as IMotionCommand);
        }

        public virtual bool Receive(IScale1DCommand command)
        {
            return Receive(command as IMotionCommand);
        }

        public virtual bool Receive(IScale2DCommand command)
        {
            return Receive(command as IMotionCommand);
        }

        public virtual bool Receive(IScaleUniformCommand command)
        {
            return Receive(command as IMotionCommand);
        }

        public virtual bool Receive(IRotate3DCommand command)
        {
            return Receive(command as IMotionCommand);
        }
    }
}