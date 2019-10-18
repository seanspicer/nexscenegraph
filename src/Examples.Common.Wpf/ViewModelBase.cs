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