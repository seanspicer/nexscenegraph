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

namespace Veldrid.SceneGraph.Manipulators
{
    public class Util
    {
        public static NodePath ComputeNodePathToRoot(INode node)
        {
            var result = new NodePath();

            var nodePaths = node.GetParentalNodePaths();
            if (!nodePaths.Any()) return result;

            result = nodePaths.First();
            if (nodePaths.Count > 1)
            {
                // TODO: Log this as degenerate case.
            }

            return result;
        }
    }
}