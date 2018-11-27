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

using System.Collections.Generic;
using System.Numerics;

namespace Veldrid.SceneGraph.InputAdapter
{
    public class InputStateSnapshot : InputSnapshot, IInputStateSnapshot
    {
        private InputSnapshot _snapshot;

        public static IInputStateSnapshot Create(InputSnapshot snapshot, int width, int height)
        {
            return new InputStateSnapshot(snapshot, width, height);
        }
        
        protected InputStateSnapshot(InputSnapshot snapshot, int width, int height)
        {
            WindowWidth = width;
            WindowHeight = height;
            _snapshot = snapshot;
        }

        public bool IsMouseDown(MouseButton button)
        {
            return _snapshot.IsMouseDown(button);
        }

        public IReadOnlyList<KeyEvent> KeyEvents => _snapshot.KeyEvents;
        public IReadOnlyList<MouseEvent> MouseEvents => _snapshot.MouseEvents;
        public IReadOnlyList<char> KeyCharPresses => _snapshot.KeyCharPresses;
        public Vector2 MousePosition => _snapshot.MousePosition;
        public float WheelDelta => _snapshot.WheelDelta;
        
        public int WindowWidth { get; private set; }
        public int WindowHeight { get; private set; }
    }
}