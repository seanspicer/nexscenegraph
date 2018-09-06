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
    public class RenderBin
    {
        public enum SortModeTypes
        {
            SortByState,
            SortByStateThenFrontToBack,
            SortFrontToBack,
            SortBackToFront,
            TraversalOrder
        };

        public SortModeTypes SortMode { get; set; } = SortModeTypes.SortByState;
        public StateGraph StateGraph { get; set; } = null;
        public StateSet StateSet { get; set; } = null;
        public RenderBin Parent { get; set; } = null;
        public RenderStage Stage { get; set; } = null;
        public uint BinNumber { get; set; } = 0;
        public Dictionary<uint, RenderBin> RenderBinDict { get; set; } = new Dictionary<uint, RenderBin>();
        public List<StateGraph> StateGraphList { get; set; } = new List<StateGraph>();
        public List<RenderLeaf> RenderLeafList { get; set; } = new List<RenderLeaf>();

        public RenderBin()
        {
            
        }

        public virtual void Draw(RenderInfo renderInfo, RenderLeaf previous)
        {
            
        }
    }
}