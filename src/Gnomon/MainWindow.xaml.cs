using System;
using System.ComponentModel;
using System.Numerics;
using System.Windows;
using Microsoft.Extensions.Logging;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.Logging;
using Math = System.Math;

namespace Gnomon
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private SceneViewModel _viewModel;
        private Matrix4x4 _gnomonStepBack = Matrix4x4.CreateTranslation(0, 0, -10.0f);

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new SceneViewModel();
            DataContext = _viewModel;
        }

        private void ChangeCameraButton_OnClick(object sender, RoutedEventArgs e)
        {
            _viewModel.ChangeCamera(VSGElement.GetUiActionAdapter(), VSGElement.GetCamera());
        }
        
        private void window_Activated(object sender, EventArgs e)
        {
            _viewModel.PropertyChanged += VmPropertyHandler;
            
            BackOff();
        }
        private void VmPropertyHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MainViewMatrix")
            {
                Matrix4x4 matrix;
                var ok = Matrix4x4.Decompose(_viewModel.MainViewMatrix, out var scale, out var rotation, out var translation);
                if (ok)
                {
                    matrix = Matrix4x4.CreateFromQuaternion(rotation);
                }
                else
                {
                    matrix = Matrix4x4.CreateFromQuaternion(new Quaternion());
                }
                matrix *= _gnomonStepBack;
                var camera = VsgElementGnomon.GetCamera();
                camera.SetViewMatrix(matrix);
            }
        }

        private INode GetGnomonRoot()
        {
            return VsgElementGnomon.SceneRoot;
        }

        private void BackOff()
        {
            var camera = VsgElementGnomon.GetCamera();
            var distance = BackOffProjection(camera, GetGnomonRoot());
            _gnomonStepBack = Matrix4x4.CreateTranslation(0, 0, -distance);
            camera.SetViewMatrix(_gnomonStepBack);
        }
        
        // copied from orbit manipulator.
        // fov was not what was expected...
        private float BackOffProjection(ICamera camera, INode node, float slack = 20)
        {
            var cbv = ComputeBoundsVisitor.Create();
            node.Accept(cbv);
                
            var bSphere = BoundingSphere.Create();
            bSphere.ExpandBy(cbv.GetBoundingBox());
            if (bSphere.Radius < 0) return 10.0f;

            // TODO: make this more general to the scene
            var radius = bSphere.Radius*2; // don't want the center; 
            var center = new Vector3(); //  origin
                
            switch (camera)
            {
                case IPerspectiveCamera perspectiveCamera:
                {
                    // Compute an aspect-radius to ensure that the 
                    // scene will be inside the viewing volume
                    var aspect = camera.Viewport.AspectRatio;
                    if (aspect >= 1.0)
                    {
                        aspect = 1.0f;
                    }

                    var aspectRadius = radius / aspect;

                    Vector3 camEye;
                    Vector3 camCenter;
                    Vector3 camUp;

                    perspectiveCamera.ProjectionMatrix.GetLookAt(out camEye, out camCenter, out camUp, 1);

                    // Compute the direction of motion for the camera
                    // between it's current position and the scene center
                    var direction = camEye - camCenter;
                    var normDirection = Vector3.Normalize(direction);

                    // Compute the length to move the camera by examining
                    // the tangent to the bounding sphere
                    var moveLen = radius + aspectRadius / (Math.Tan(perspectiveCamera.VerticalFov / 2.0));

                    // Compute the new camera position
                    var moveDirection = normDirection * (float) moveLen;
                    var cameraPos = center + moveDirection;

                    // Compute the near and far plane locations
                    const double epsilon = 0.001;
                    var distToMid = (cameraPos - center).Length();
                    var zNear = (float) Math.Max(distToMid * epsilon, distToMid - radius * slack);
                    var zFar = distToMid + radius * slack;

                    // _center = center;
                    // _distance = distToMid;

                    perspectiveCamera.SetProjectionMatrixAsPerspective(perspectiveCamera.VerticalFov,
                        perspectiveCamera.Viewport.AspectRatio, zNear, zFar);
                    return distToMid;
                }
                case IOrthographicCamera orthoCamera:
                {
                    return 10.0f;
                }
                default:
                    return 10.0f;
            }
        }
    }
}