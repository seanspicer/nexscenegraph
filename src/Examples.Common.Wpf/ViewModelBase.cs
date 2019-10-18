using System.ComponentModel;
using System.Runtime.CompilerServices;
using Examples.Common.Wpf.Annotations;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.InputAdapter;

namespace Examples.Common.Wpf
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        private IGroup _sceneRoot;
        public IGroup SceneRoot
        {
            get => _sceneRoot;
            set
            {
                _sceneRoot = value;
                OnPropertyChanged("SceneRoot");
            }
        }

        private ICameraManipulator _cameraManipulator;

        public ICameraManipulator CameraManipulator
        {
            get => _cameraManipulator;
            set
            {
                _cameraManipulator = value;
                OnPropertyChanged("CameraManipulator");
            }
        }

        private IInputEventHandler _eventHandler;

        public IInputEventHandler EventHandler
        {
            get => _eventHandler;
            set
            {
                _eventHandler = value;
                OnPropertyChanged("EventHandler");
            }
        }

        protected ViewModelBase()
        {
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}