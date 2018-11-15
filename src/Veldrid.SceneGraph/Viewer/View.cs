//
// Copyright (c) 2018 Sean Spicer
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

using System;
using Veldrid.SceneGraph.InputAdapter;

namespace Veldrid.SceneGraph.Viewer
{
    public class View : Veldrid.SceneGraph.View
    {
        public IGroup SceneData { get; set; }

        private Action<InputStateSnapshot> HandleInputSnapshot;

        private CameraManipulator _cameraManipulator = null;
        public CameraManipulator CameraManipulator
        {
            get => _cameraManipulator;
            set
            {
                if (null != _cameraManipulator)
                {
                    throw new Exception("Setting camera manipulator twice.  Don't do that.");
                }
                _cameraManipulator = value;
                HandleInputSnapshot += _cameraManipulator.HandleInput;
            }
        }

        private PickHandler _pickHandler = null;

        public PickHandler PickHandler
        {
            get => _pickHandler;
            set
            {
                if (null != _pickHandler)
                {
                    throw new Exception("Setting camera manipulator twice.  Don't do that.");
                }

                _pickHandler = value;
                HandleInputSnapshot += _pickHandler.HandleInput;
            }
        }
    
        public View()
        {
            Camera.Renderer = new Renderer(Camera);
        }

        public void OnInputEvent(InputStateSnapshot snapshot)
        {
            HandleInputSnapshot?.Invoke(snapshot);

            // Update the camera on input
            _cameraManipulator?.UpdateCamera(Camera);

        }
    }
}