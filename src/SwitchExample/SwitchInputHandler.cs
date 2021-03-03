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

using Veldrid.SceneGraph;
using Veldrid.SceneGraph.InputAdapter;

namespace SwitchExample
{
    public class SwitchInputHandler : UiEventHandler
    {
        private int _pos;

        private readonly ISwitch _switch;
//

        public SwitchInputHandler(ISwitch switchNode)
        {
            //_logger = LogManager.GetLogger<SwitchInputHandler>();
            _switch = switchNode;
            _pos = -1;
        }

        public override bool Handle(IUiEventAdapter eventAdapter, IUiActionAdapter uiActionAdapter)
        {
            switch (eventAdapter.Key)
            {
                case IUiEventAdapter.KeySymbol.KeyS:
                    DoSwitch();
                    return true;
                default:
                    return false;
            }
        }

        private void DoSwitch()
        {
            if (_pos == -1)
                _switch.SetAllChildrenOff();
            // _logger.Info(m => m("All Children Off"));
            else if (_pos == _switch.GetNumChildren())
                _switch.SetAllChildrenOn();
            //_logger.Info(m => m("All Children On"));
            else
                _switch.SetValue(_pos, true);
            //_logger.Info(m => m($"Enabled Child At => {_pos}"));

            _pos++;
            if (_pos == _switch.GetNumChildren()) _pos = -1;
        }
    }
}