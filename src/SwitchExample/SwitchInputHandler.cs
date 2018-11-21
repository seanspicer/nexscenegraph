using SharpDX.DXGI;
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.InputAdapter;

namespace SwitchExample
{
    public class SwitchInputHandler : InputEventHandler
    {
        private ISwitch _switch;
        private int _pos;
        
        public SwitchInputHandler(ISwitch switchNode)
        {
            _switch = switchNode;
            _pos = -1;
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
                        case Key.S:
                            DoSwitch(snapshot);
                            break;
                    }
                }
            }
        }

        private void DoSwitch(IInputStateSnapshot snapshot)
        {
            if (_pos == -1)
            {
                _switch.SetAllChildrenOff();
                
            }
            else if (_pos == _switch.GetNumChildren())
            {
                _switch.SetAllChildrenOn();
            }
            else
            {
                _switch.SetValue(_pos, true);
            }

            _pos++;
            if (_pos == _switch.GetNumChildren())
            {
                _pos = -1;
            }
        }
    }
}