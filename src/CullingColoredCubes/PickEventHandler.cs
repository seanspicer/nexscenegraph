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
using Serilog;
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Util;

namespace CullingColoredCubes
{
    public class PickEventHandler : InputEventHandler
    {
        private Veldrid.SceneGraph.Viewer.IView _view;

        private readonly ILogger _logger;
        
        public PickEventHandler(Veldrid.SceneGraph.Viewer.IView view)
        {
            _logger = Log.Logger.ForContext("Source", "CullingColoredCubes");
            _view = view;
        }
        
        public override void HandleInput(IInputStateSnapshot snapshot)
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

        private void DoPick(IInputStateSnapshot snapshot)
        {
            var norm = GetNormalizedMousePosition();
            
            var startPos = _view.Camera.NormalizedScreenToWorld(new Vector3(norm.X, norm.Y, 0.0f)); // Near plane
            var endPos = _view.Camera.NormalizedScreenToWorld(new Vector3(norm.X, norm.Y, 1.0f)); // Far plane
            var intersector = LineSegmentIntersector.Create(startPos, endPos);
            
            var intersectionVisitor = IntersectionVisitor.Create(intersector);
            
            _view.SceneData?.Accept(intersectionVisitor);

            if (intersector.Intersections.Any())
            {
                var idx = 0;
                foreach (var intersection in intersector.Intersections)
                {
                    _logger.Information($"Intersected [{idx}]: {intersection.Drawable.Name}");
                    var jdx = 0;
                    foreach (var node in intersection.NodePath)
                    {
                        _logger.Information($"  Path[{jdx}]: {node.NameString}");
                        ++jdx;
                    }
                    ++idx;
                }
                
            }
            else
            {
                _logger.Information("No Intersections");
            }
        }
    }
}