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

using System.Linq;
using System.Numerics;
using Examples.Common;
using Microsoft.Extensions.Logging;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.Viewer;

namespace CullingColoredCubes
{
    public class PickEventHandler : UiEventHandler
    {
        private readonly ILogger _logger;

        public PickEventHandler()
        {
            _logger = Bootstrapper.LoggerFactory.CreateLogger("PickEventHandler");
        }

        public override bool Handle(IUiEventAdapter eventAdapter, IUiActionAdapter uiActionAdapter)
        {
            switch (eventAdapter.Key)
            {
                case IUiEventAdapter.KeySymbol.KeyP:
                    DoPick(eventAdapter, uiActionAdapter as IView);
                    ;
                    return true;
                default:
                    return false;
            }
        }

        private void DoPick(IUiEventAdapter eventAdapter, IView view)
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
                    _logger.LogInformation($"Intersected [{idx}]: {intersection.Drawable.Name}");
                    var jdx = 0;
                    foreach (var node in intersection.NodePath)
                    {
                        _logger.LogInformation($"  Path[{jdx}]: {node.NameString}");
                        ++jdx;
                    }

                    ++idx;
                }
            }
            else
            {
                _logger.LogInformation("No Intersections");
            }
        }
    }
}