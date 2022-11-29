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

using System;
using System.Linq;
using System.Numerics;
using System.Windows.Controls;
using Examples.Common.Wpf;
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Util;
using IView = Veldrid.SceneGraph.Viewer.IView;

namespace WpfDemo
{
    public class PickEventHandler : FrameCaptureEventHandler
    {
        private bool _isOrthoGraphic = false;
        
        public PickEventHandler()
        {
        }
        
        public override bool Handle(IUiEventAdapter eventAdapter, IUiActionAdapter uiActionAdapter)
        {
            if (true == base.Handle(eventAdapter, uiActionAdapter))
            {
                return true;
            }
            
            switch (eventAdapter.Key)
            {
                case IUiEventAdapter.KeySymbol.KeyP:
                    DoPick(eventAdapter, uiActionAdapter as Veldrid.SceneGraph.Viewer.IView);
                    return true;
                case IUiEventAdapter.KeySymbol.KeyO:
                    if (!_isOrthoGraphic)
                    {
                        var view = uiActionAdapter as Veldrid.SceneGraph.Viewer.IView;
                        var camera = view.Camera;
                        var width = camera.Width;
                        var height = camera.Height;
                        var dist = camera.Distance;
                        view.SetCamera(OrthographicCameraOperations.CreateOrthographicCamera(width, height, dist));
                        _isOrthoGraphic = true;
                    }
                    return true;
                case IUiEventAdapter.KeySymbol.KeyR:
                    if (_isOrthoGraphic)
                    {
                        var view = uiActionAdapter as Veldrid.SceneGraph.Viewer.IView;
                        var camera = view.Camera;
                        var width = camera.Width;
                        var height = camera.Height;
                        var dist = camera.Distance;
                        view.SetCamera(PerspectiveCameraOperations.CreatePerspectiveCamera(width, height, dist));
                        _isOrthoGraphic = false;
                    }
                    return true;
                
                default:
                    return false;
            }
            
        }

        private void DoPick(IUiEventAdapter eventAdapter, Veldrid.SceneGraph.Viewer.IView view)
        {
            var norm = new Vector2(eventAdapter.XNormalized, eventAdapter.YNormalized);
            
            var startPos = view.Camera.NormalizedScreenToWorld(new Vector3(norm.X, norm.Y, 0.0f)); // Near plane
            var endPos = view.Camera.NormalizedScreenToWorld(new Vector3(norm.X, norm.Y, 1.0f)); // Far plane
            var intersector = LineSegmentIntersector.Create(startPos, endPos);
            
            var intersectionVisitor = IntersectionVisitor.Create(intersector);
            
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