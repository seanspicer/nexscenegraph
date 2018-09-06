//
// Copyright (c) 2018 Sean Spicer
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

using System;

namespace Veldrid.SceneGraph.Util
{
    public class CullVisitor : NodeVisitor
    {
        private CullStack _cullStack = new CullStack();
        
        private StateGraph _rootStateGraph = null;
        private StateGraph _currentStateGraph = null;

        private RenderStage _rootRenderStage = null;
        private RenderBin _currentRenderBin = null;
        
        private float _computedZNear = float.PositiveInfinity;
        private float _computedZFar = -float.NegativeInfinity;

        private int _traversalOrderNumber = 0;
        private int _currentReuseRenderLeafIndex = 0;
        private int _numberOfEncloseOverrideRenderBinDetails = 0;

        public StateGraph RootStateGraph
        {
            get => _rootStateGraph;
        }
        
        public StateGraph CurrentStateGraph
        {
            get => _currentStateGraph;
        }
        
        public RenderStage RenderStage
        {
            get => _rootRenderStage;
        }
        public RenderStage CurrentRenderStage
        {
            get => _currentRenderBin.Stage;
        }

        public RenderBin CurrentRenderBin => _currentRenderBin;
        
        public Camera CurrentCamera
        {
            get => CurrentRenderStage.Camera;
        }
        

        public CullVisitor() : base(VisitorType.CullVisitor, TraversalModeType.TraverseActiveChildren)
        {
            // Nothing here (yet!)
        }

        public void SetStateGraph(StateGraph stateGraph)
        {
            _rootStateGraph = stateGraph;
            _currentStateGraph = stateGraph;
        }
        
        public override void Apply(Drawable drawable)
        {
            var matrix = _cullStack.GetProjectionMatrix();

            var bb = drawable.GetBoundingBox();
        }
        
        public override void Apply<T>(Geometry<T> node)
        {
            Apply((Node)node);
        }
    }
}