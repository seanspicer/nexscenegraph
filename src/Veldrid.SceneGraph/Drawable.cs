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

namespace Veldrid.SceneGraph
{
    public abstract class Drawable : Node
    {
        protected BoundingBox _boundingBox;
        protected BoundingBox _initialBoundingBox = new BoundingBox();
        public BoundingBox InitialBoundingBox
        {
            get => _initialBoundingBox;
            set
            {
                _initialBoundingBox = value;
                DirtyBound();
            }
        } 
        
        public event Func<Node, BoundingBox> ComputeBoundingBoxCallback;
        public event Action<CommandList, Drawable> DrawImplementationCallback;

        public void Draw(CommandList commandList)
        {
            if (null != DrawImplementationCallback)
            {
                DrawImplementationCallback(commandList, this);
            }
            else
            {
                DrawImplementation(commandList);
            }
        }

        protected abstract void DrawImplementation(CommandList commandList);
        
        public BoundingBox GetBoundingBox()
        {
            if (_boundingSphereComputed) return _boundingBox;
            
            _boundingBox = _initialBoundingBox;

            _boundingBox.ExpandBy(null != ComputeBoundingBoxCallback
                ? ComputeBoundingBoxCallback(this)
                : ComputeBoundingBox());

            if (_boundingBox.Valid())
            {
                _boundingSphere.Set(_boundingBox.Center, _boundingBox.Radius);
            }
            else
            {
                _boundingSphere.Init();
            }

            _boundingSphereComputed = true;

            return _boundingBox;
        }

        protected abstract BoundingBox ComputeBoundingBox();
    }
}