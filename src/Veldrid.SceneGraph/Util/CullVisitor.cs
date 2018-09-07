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
using System.Linq;
using System.Numerics;

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

            // TODO Add Cull Callback Here

            if (drawable.IsCullingActive && _cullStack.IsCulled(bb)) return;

            if (CullSettings.ComputeNearFarMode.DoNotComputeNearFar != _cullStack.GetComputeNearFarMode() && bb.Valid())
            {
                if (!UpdateCalculatedNearFar(matrix, drawable, false))
                {
                    return;
                }
            }

            // need to track how push/pops there are, so we can unravel the stack correctly.
            uint numPopStateSetRequired = 0;

            // push the geoset's state on the geostate stack.
            var stateSet = drawable.StateSet;
            if (null != stateSet)
            {
                ++numPopStateSetRequired;
                PushStateSet(stateSet);
            }

            var cs = _cullStack.CurrentCullingSet;
            foreach (var sf in cs.StateFrustumList)
            {
                if (sf.Polytope.Contains(bb))
                {
                    ++numPopStateSetRequired;
                    PushStateSet(stateSet);
                }
            }
            
            var depth = bb !=null ? Distance(bb.Center, matrix) : 0.0f;

            if (float.IsNaN(depth))
            {
                Console.WriteLine("CullVisitor.Apply(Drawable) detected NaN");
            }
            else
            {
                AddDrawableAndDepth(drawable, matrix, depth);
            }
            
            for(var i=0;i< numPopStateSetRequired; ++i)
            {
                PopStateSet();
            }

        }

        private void PushStateSet(StateSet stateSet)
        {
            // TODO - Deal with RenderBins?
            _currentStateGraph = _currentStateGraph.FindOrInsert(stateSet);
        }
        
        private void PopStateSet()
        {
           // TODO - Deal with RenderBins?
            _currentStateGraph = _currentStateGraph.Parent;
        }

        private void AddDrawableAndDepth(Drawable drawable, Matrix4x4 matrix, float depth)
        {
            if (_currentStateGraph.Leaves.Count == 0)
            {
                _currentRenderBin.StateGraphList.Add(_currentStateGraph);
            }
            _currentStateGraph.AddLeaf(new RenderLeaf(drawable, _cullStack.GetProjectionMatrix(),matrix,depth));
        }

        private bool UpdateCalculatedNearFar(Matrix4x4 matrix, Drawable drawable, bool isBillboard)
        {
            var bb = drawable.GetBoundingBox();

            if (isBillboard)
            {
                throw new NotImplementedException();
            }

            // Brute force
            UpdateCalculatedNearFar(bb.Corner(0));
            UpdateCalculatedNearFar(bb.Corner(1));
            UpdateCalculatedNearFar(bb.Corner(2));
            UpdateCalculatedNearFar(bb.Corner(3));
            UpdateCalculatedNearFar(bb.Corner(4));
            UpdateCalculatedNearFar(bb.Corner(5));
            UpdateCalculatedNearFar(bb.Corner(6));
            UpdateCalculatedNearFar(bb.Corner(7));

            return true;

        }

        private void UpdateCalculatedNearFar(Vector3 pos)
        {
            float d;
            if (_cullStack.ModelViewStack.Count != 0)
            {
                var matrix = _cullStack.ModelViewStack.Peek();
                d = Distance(pos, matrix);
            }
            else
            {
                d = -pos.Z;
            }

            if (d < _computedZNear)
            {
                _computedZNear = d;
                if (d < 0.0)
                {
                    // Billboard?
                    throw new Exception("Alerting billboard");
                }
            }

            if (d > _computedZFar)
            {
                _computedZFar = d;
            }
        }

        private float Distance(Vector3 coord, Matrix4x4 matrix)
        {
            return -(coord.X*matrix.M13+coord.Y*matrix.M23+coord.Z*matrix.M33+matrix.M43);
 
        }

        public override void Apply<T>(Geometry<T> node)
        {
            Apply((Node)node);
        }
    }
}