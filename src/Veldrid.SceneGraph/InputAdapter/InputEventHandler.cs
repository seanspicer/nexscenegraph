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

using System.Numerics;

namespace Veldrid.SceneGraph.InputAdapter
{
    public abstract class InputEventHandler : IInputEventHandler
    {
        protected InputStateTracker InputStateTracker { get; }= new InputStateTracker();

        public virtual void SetView(IView view)
        {
            
        }

        public virtual void HandleInput(IInputStateSnapshot snapshot)
        {
            InputStateTracker.UpdateFrameInput(snapshot);
        }

        protected Vector2 GetNormalizedMousePosition()
        {
            var xNorm = 2.0f*(InputStateTracker.MousePosition.Value.X / InputStateTracker.FrameSnapshot.WindowWidth)-1.0f;
            var yNorm = -2.0f*(InputStateTracker.MousePosition.Value.Y / InputStateTracker.FrameSnapshot.WindowHeight)+1.0f;

            return new Vector2(xNorm, yNorm);
        }
    }
    
}