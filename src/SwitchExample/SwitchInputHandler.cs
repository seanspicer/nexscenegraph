using Common.Logging;
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

        private ILog _logger;
        
        public SwitchInputHandler(ISwitch switchNode)
        {
            _logger = LogManager.GetLogger<SwitchInputHandler>();
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
                _logger.Info(m => m("All Children Off"));
                
            }
            else if (_pos == _switch.GetNumChildren())
            {
                _switch.SetAllChildrenOn();
                _logger.Info(m => m("All Children On"));
            }
            else
            {
                _switch.SetValue(_pos, true);
                _logger.Info(m => m($"Enabled Child At => {_pos}"));
            }

            _pos++;
            if (_pos == _switch.GetNumChildren())
            {
                _pos = -1;
            }
        }
    }
}