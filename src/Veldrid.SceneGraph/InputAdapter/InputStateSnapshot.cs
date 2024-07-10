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
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Veldrid.SceneGraph.InputAdapter
{
    // internal class EmptyInputSnapshot : InputSnapshot
    // {
    //     public EmptyInputSnapshot(ReadOnlySpan<Rune> inputEvents)
    //     {
    //         InputEvents = inputEvents;
    //     }
    //
    //     public ReadOnlySpan<Rune> InputEvents { get; }
    //     public ReadOnlySpan<KeyEvent> KeyEvents { get; }
    //     public ReadOnlySpan<MouseButtonEvent> MouseEvents { get; }
    //     public Vector2 MousePosition { get; }
    //     public Vector2 WheelDelta { get; }
    //     public MouseButton MouseDown { get; }
    // }

    public class InputStateSnapshot : InputSnapshot, IInputStateSnapshot
    {
        private readonly InputSnapshot _snapshot;

        protected InputStateSnapshot(InputSnapshot snapshot, int width, int height, Matrix4x4 projectionMatrix,
            Matrix4x4 viewMatrix)
        {
            WindowWidth = width;
            WindowHeight = height;
            ProjectionMatrix = projectionMatrix;
            ViewMatrix = viewMatrix;
            _snapshot = snapshot;
        }

        public int WindowWidth { get; }
        public int WindowHeight { get; }
        public Matrix4x4 ProjectionMatrix { get; }
        public Matrix4x4 ViewMatrix { get; }

        public MouseButton MouseDown => _snapshot.MouseDown;

        public ReadOnlySpan<KeyEvent> KeyEvents => _snapshot.KeyEvents;
        public ReadOnlySpan<MouseButtonEvent> MouseEvents => _snapshot.MouseEvents;
        public ReadOnlySpan<Rune> InputEvents => _snapshot.InputEvents;
        public Vector2 MousePosition => _snapshot.MousePosition;
        public Vector2 WheelDelta => _snapshot.WheelDelta;

        public static InputStateSnapshot CreateEmpty(int width = 0, int height = 0)
        {
            throw new NotImplementedException();
            // return new InputStateSnapshot(new EmptyInputSnapshot(), width, height, Matrix4x4.Identity,
            //     Matrix4x4.Identity);
        }

        public static IInputStateSnapshot Create(InputSnapshot snapshot, int width, int height,
            Matrix4x4 projectionMatrix, Matrix4x4 viewMatrix)
        {
            return new InputStateSnapshot(snapshot, width, height, projectionMatrix, viewMatrix);
        }
    }
}