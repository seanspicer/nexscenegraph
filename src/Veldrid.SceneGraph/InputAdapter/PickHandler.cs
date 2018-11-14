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
using System.Linq;
using System.Numerics;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph.InputAdapter
{
    public class PickHandler : InputEventHandler
    {
        private Camera _camera;
        
        public PickHandler(Camera camera)
        {
            _camera = camera;
        }
        
        public void HandleInput(InputStateSnapshot snapshot)
        {
            base.HandleInput(snapshot);
            
            foreach (var keyEvent in snapshot.KeyEvents)
            {
                if (keyEvent.Down)
                {
                    switch (keyEvent.Key)
                    {
                        case Key.P:
                            DoPick(snapshot);
                            break;
                    }
                }
            }
        }
        
        private void DoPick(InputStateSnapshot snapshot)
        {
            var norm = GetNormalizedMousePosition();
            
            var startPos = _camera.NormalizedScreenToWorld(new Vector3(norm.X, norm.Y, 0.0f)); // Near plane
            var endPos = _camera.NormalizedScreenToWorld(new Vector3(norm.X, norm.Y, 1.0f)); // Far plane
            var intersector = new LineSegmentIntersector(startPos, endPos);
            
            var intersectionVisitor = new IntersectionVisitor(intersector);
            
            var view = (Viewer.View) _camera.View;
            view.SceneData?.Accept(intersectionVisitor);

            if (intersector.Intersections.Any())
            {
                var idx = 0;
                foreach (var intersection in intersector.Intersections)
                {
                    Console.WriteLine($"Intersected [{idx}]: {intersection.Drawable.Name}");
                    var jdx = 0;
                    foreach (var node in intersection.NodePath)
                    {
                        Console.WriteLine($"  Path[{jdx}]: {node.NameString}");
                        ++jdx;
                    }
                    ++idx;
                }
                
            }
            else
            {
                Console.WriteLine("No Intersections");
            }
            
            
            
        }
    }
}