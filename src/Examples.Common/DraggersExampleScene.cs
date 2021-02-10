

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
            
            var scale2DDragger = Scale2DDragger.Create(IScale2DDragger.ScaleMode.ScaleWithOppositeHandleAsPivot);
            scale2DDragger.SetupDefaultGeometry();
            scale2DDragger.HandleEvents = true;
            var scale2DDraggerXForm = MatrixTransform.Create(Matrix4x4.CreateTranslation(1f, -1.0f, 0.0f)); //(1f, -1.0f, 0.0f));
            scale2DDraggerXForm.AddChild(scale2DDragger);
            root.AddChild(scale2DDraggerXForm);
            
            var translate2DDragger = Translate2DDragger.Create();
            translate2DDragger.SetupDefaultGeometry();
            translate2DDragger.HandleEvents = true;
            var translate2DDraggerXForm = MatrixTransform.Create(Matrix4x4.CreateTranslation(1f, 1.0f, 0.0f));
            translate2DDraggerXForm.AddChild(translate2DDragger);
            root.AddChild(translate2DDraggerXForm);
            
            var translatePlaneDragger = TranslatePlaneDragger.Create();
            translatePlaneDragger.SetupDefaultGeometry();
            translatePlaneDragger.HandleEvents = true;
            var translatePlaneDraggerXForm = MatrixTransform.Create(Matrix4x4.CreateTranslation(1f, 1.0f, 2.0f)); //(1f, 1.0f, 2.0f)
            translatePlaneDraggerXForm.AddChild(translatePlaneDragger);
            root.AddChild(translatePlaneDraggerXForm);
            
            var tabPlaneDragger = TabPlaneDragger.Create();
            tabPlaneDragger.SetupDefaultGeometry();
            tabPlaneDragger.HandleEvents = true;
            var tabPlaneDraggerXForm = MatrixTransform.Create(Matrix4x4.CreateTranslation(-1f, 1.0f, 2.0f)); //(1f, 1.0f, 2.0f)
            tabPlaneDraggerXForm.AddChild(tabPlaneDragger);
            root.AddChild(tabPlaneDraggerXForm);
            //
            // var tabBoxDragger = TabBoxDragger.Create();
            // tabBoxDragger.SetupDefaultGeometry();
            // tabBoxDragger.HandleEvents = true;
            // var tabBoxDraggerXForm = MatrixTransform.Create(Matrix4x4.CreateTranslation(1f, -1.0f, 2.0f)); 
            // tabBoxDraggerXForm.AddChild(tabBoxDragger);
            // root.AddChild(tabBoxDraggerXForm);
            
            return root;
        }
    }
}