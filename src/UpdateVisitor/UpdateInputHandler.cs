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

using Common.Logging;
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.InputAdapter;

namespace UpdateVisitor
{
    public class UpdateInputHandler : InputEventHandler
    {
        private IGroup _root;

        private ILog _logger;
        
        public UpdateInputHandler(IGroup rootNode)
        {
            _logger = LogManager.GetLogger<UpdateInputHandler>();
            _root = rootNode;
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
                        case Key.U:
                            DoSwitchUpateState(snapshot);
                            break;
                    }
                }
            }
        }

        private void DoSwitchUpateState(IInputStateSnapshot snapshot)
        {
 
        }
    }
}