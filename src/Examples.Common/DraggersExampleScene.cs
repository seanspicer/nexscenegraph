

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
            
            var scale1DDragger = Scale1DDragger.Create(IScale1DDragger.ScaleMode.ScaleWithOppositeHandleAsPivot);
            scale1DDragger.SetupDefaultGeometry();
            scale1DDragger.HandleEvents = true;
            var scale1DDraggerXForm = MatrixTransform.Create(Matrix4x4.CreateTranslation(-1f, 1.0f, 0.0f));
            scale1DDraggerXForm.AddChild(scale1DDragger);
            root.AddChild(scale1DDraggerXForm);
            
            
            var translate1DDragger = Translate1DDragger.Create();
            translate1DDragger.SetupDefaultGeometry();
            translate1DDragger.HandleEvents = true;
            var translate1DDraggerXForm = MatrixTransform.Create(Matrix4x4.CreateTranslation(-1f, -1.0f, 0.0f));
            translate1DDraggerXForm.AddChild(translate1DDragger);
            root.AddChild(translate1DDraggerXForm);
            //
            // var scale2DDragger = Scale2DDragger.Create();
            // scale2DDragger.SetupDefaultGeometry();
            // var scale2DDraggerXForm = MatrixTransform.Create(Matrix4x4.CreateTranslation(1f, -1.0f, 0.0f));
            // scale2DDraggerXForm.AddChild(scale2DDragger);
            // root.AddChild(scale2DDraggerXForm);
            //
            // var translate2DDragger = Translate2DDragger.Create();
            // translate2DDragger.SetupDefaultGeometry();
            // var translate2DDraggerXForm = MatrixTransform.Create(Matrix4x4.CreateTranslation(1f, 1.0f, 0.0f));
            // translate2DDraggerXForm.AddChild(translate2DDragger);
            // root.AddChild(translate2DDraggerXForm);
            //
            // var tabPlaneDragger = TabPlaneDragger.Create();
            // tabPlaneDragger.SetupDefaultGeometry();
            // var tabPlaneDraggerXForm = MatrixTransform.Create(Matrix4x4.CreateTranslation(1f, 1.0f, 2.0f));
            // tabPlaneDraggerXForm.AddChild(tabPlaneDragger);
            // root.AddChild(tabPlaneDraggerXForm);
            //
            var tabBoxDragger = TabBoxDragger.Create();
            tabBoxDragger.SetupDefaultGeometry();
            var tabBoxDraggerXForm = MatrixTransform.Create(Matrix4x4.CreateTranslation(1f, -1.0f, 2.0f));
            tabBoxDraggerXForm.AddChild(tabBoxDragger);
            root.AddChild(tabBoxDraggerXForm);
            
            return root;
        }
    }
}