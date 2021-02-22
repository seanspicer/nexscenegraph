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

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Examples.Common.Wpf.Annotations;
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.InputAdapter;

namespace Examples.Common.Wpf
{
    public class FrameCaptureEventHandler : UiEventHandler
    {
        public override bool Handle(IUiEventAdapter eventAdapter, IUiActionAdapter actionAdapter)
        {
            if (actionAdapter is Veldrid.SceneGraph.Viewer.IView view)
            {
                if (eventAdapter.Key == IUiEventAdapter.KeySymbol.KeyC &&
                    (eventAdapter.EventType & IUiEventAdapter.EventTypeValue.KeyDown) != 0 &&
                    (eventAdapter.ModKeyMask & IUiEventAdapter.ModKeyMaskType.ModKeyShift) != 0 &&
                    (eventAdapter.ModKeyMask & IUiEventAdapter.ModKeyMaskType.ModKeyCtl) != 0)
                {
                    view?.Camera?.Renderer.CaptureNextFrame();
                    return true;
                }
            }
            return false;
        }
    }
    
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

        private IUiEventHandler _eventHandler;

        public IUiEventHandler EventHandler
        {
            get => _eventHandler;
            set
            {
                _eventHandler = value;
                OnPropertyChanged("EventHandler");
            }
        }

        private RgbaFloat _clearColor;

        public RgbaFloat ClearColor
        {
            get => _clearColor;
            set
            {
                _clearColor = value;
                OnPropertyChanged("ClearColor");
            }
        }

        private TextureSampleCount _fssaCount;

        public TextureSampleCount FsaaCount
        {
            get => _fssaCount;
            set
            {
                _fssaCount = value;
                OnPropertyChanged("FsaaCount");
            }
        }
        
        protected ViewModelBase()
        {
            FsaaCount = TextureSampleCount.Count16;
            EventHandler = new FrameCaptureEventHandler();
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}