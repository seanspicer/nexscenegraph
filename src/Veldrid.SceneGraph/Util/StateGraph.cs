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

using System.Collections.Generic;

namespace Veldrid.SceneGraph.Util
{
    public class StateGraph
    {
        public StateGraph Parent { get; set; } = null;
        public StateSet StateSet { get; set; } = null;

        private int _depth = 0;

        public static void MoveStateGraph(State state, StateGraph sgCurr, StateGraph sgNew)
        {
            if (sgNew == sgCurr || null == sgNew) return;

            if (sgCurr == null)
            {
                var returnPath = new Stack<StateGraph>();
                while (null != sgNew)
                {
                    returnPath.Push(sgNew);
                    sgNew = sgNew.Parent;
                }

                foreach (var rg in returnPath)
                {
                    if (null != rg.StateSet)
                    {
                        state.PushStateSet(rg.StateSet);
                    }
                }

                return;
            }
            
            if(sgCurr.Parent == sgNew.Parent)
            {
                if (null != sgCurr.StateSet)
                {
                    state.PopStateSet();
                }

                if (null != sgNew.StateSet)
                {
                    state.PushStateSet(sgNew.StateSet);
                }

                return;
            }

            while (sgCurr._depth > sgNew._depth)
            {
                if(null != sgCurr.StateSet) state.PopStateSet();
                sgCurr = sgCurr.Parent;
            }
            
            var rp = new Stack<StateGraph>();
            
            while (sgNew._depth > sgCurr._depth)
            {
                rp.Push(sgNew);
                sgNew = sgNew.Parent;
            }

            while (sgCurr != sgNew)
            {
                if (null != sgCurr.StateSet)
                {
                    state.PopStateSet();
                    sgCurr = sgCurr.Parent;
                }
                
                rp.Push(sgNew);
                sgNew = sgNew.Parent;
            }

            foreach (var rg in rp)
            {
                if (null != rg.StateSet)
                {
                    state.PushStateSet(rg.StateSet);
                }
            }
        }
    }
}