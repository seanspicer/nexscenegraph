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
using System.Windows;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.InputAdapter;

namespace WpfDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private MainWindowViewModel _viewModel;
        private bool _isOrthoGraphic = false;
        
        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;
        }

        private void ChangeCameraButton_OnClick(object sender, RoutedEventArgs e)
        {
            var camera = VSGElement.GetCamera();
            
            var width = camera.Width;
            var height = camera.Height;
            var dist = camera.Distance;
            if (!_isOrthoGraphic)
            {
                _viewModel.SetCameraOrthographic(VSGElement.GetCamera());//OrthographicCameraOperations.ConvertFromPerspectiveToOrthographic(VSGElement.GetCamera()));
                _isOrthoGraphic = true;
            }
            else
            {
                _viewModel.SetCameraPerspective(VSGElement.GetCamera());//PerspectiveCameraOperations.ConvertFromOrthographicToPerspective(VSGElement.GetCamera()));
                _isOrthoGraphic = false;
            }
        }
        
        private void ChangeCameraViewButton_OnClick(object sender, RoutedEventArgs e)
        {
            _viewModel.ChangeCamera(VSGElement.GetUiActionAdapter(), VSGElement.GetCamera());
        }

        private void ViewAllButton_OnClick(object sender, RoutedEventArgs e)
        {
            VSGElement.CameraManipulator.ViewAll(VSGElement.GetUiActionAdapter());
        }
    }
}