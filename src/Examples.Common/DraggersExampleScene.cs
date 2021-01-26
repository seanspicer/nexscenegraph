

using System.Numerics;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.Manipulators;
using Veldrid.SceneGraph.NodeKits.DirectVolumeRendering;

namespace Examples.Common
{
    public class DraggersExampleScene
    {
        public static IGroup Build()
        {
            var root = Group.Create();
            
            var scale1DDragger = Scale1DDragger.Create();
            scale1DDragger.SetupDefaultGeometry();
            var scale1DDraggerXForm = MatrixTransform.Create(Matrix4x4.CreateTranslation(-1f, 1.0f, 0.0f));
            scale1DDraggerXForm.AddChild(scale1DDragger);
            root.AddChild(scale1DDraggerXForm);
            
            var translate1DDragger = Translate1DDragger.Create();
            translate1DDragger.SetupDefaultGeometry();
            var translate1DDraggerXForm = MatrixTransform.Create(Matrix4x4.CreateTranslation(-1f, -1.0f, 0.0f));
            translate1DDraggerXForm.AddChild(translate1DDragger);
            root.AddChild(translate1DDraggerXForm);
            
            var scale2DDragger = Scale2DDragger.Create();
            scale2DDragger.SetupDefaultGeometry();
            var scale2DDraggerXForm = MatrixTransform.Create(Matrix4x4.CreateTranslation(1f, -1.0f, 0.0f));
            scale2DDraggerXForm.AddChild(scale2DDragger);
            root.AddChild(scale2DDraggerXForm);
            
            var translate2DDragger = Translate2DDragger.Create();
            translate2DDragger.SetupDefaultGeometry();
            var translate2DDraggerXForm = MatrixTransform.Create(Matrix4x4.CreateTranslation(1f, 1.0f, 0.0f));
            translate2DDraggerXForm.AddChild(translate2DDragger);
            root.AddChild(translate2DDraggerXForm);
            
            return root;
        }
    }
}