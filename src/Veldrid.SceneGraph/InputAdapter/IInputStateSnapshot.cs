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
    public interface IInputStateSnapshot
    {
        ReadOnlySpan<KeyEvent> KeyEvents { get; }
        ReadOnlySpan<MouseButtonEvent> MouseEvents { get; }
        ReadOnlySpan<Rune> InputEvents { get; }
        Vector2 MousePosition { get; }
        Vector2 WheelDelta { get; }
        int WindowWidth { get; }
        int WindowHeight { get; }

        Matrix4x4 ProjectionMatrix { get; }
        Matrix4x4 ViewMatrix { get; }
        MouseButton MouseDown { get; }
    }
}