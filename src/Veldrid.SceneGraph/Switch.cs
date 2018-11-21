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
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Veldrid.SceneGraph
{
    public class Switch : Group, ISwitch
    {
        private List<bool> switchVals = new List<bool>();
        
        public static ISwitch Create()
        {
            return new Switch();
        }

        public override void Traverse(INodeVisitor nv)
        {
            foreach (var child in _children)
            {
                if (child.Item2)
                {
                    child.Item1.Accept(nv);
                }
            }
        }

        public void SetValue(int pos, bool value)
        {
            _children[pos] = Tuple.Create(_children[pos].Item1, value);
        }

        public bool GetValue(int pos, bool value)
        {
            return _children[pos].Item2;
        }

        public void SetChildValue(INode child, bool value)
        {
            for (var i = 0; i < _children.Count; ++i)
            {
                if (_children[i].Item1.Id == child.Id)
                {
                    SetValue(i, value);
                }
            }
        }

        public bool GetChildValue(INode child, bool value)
        {
            foreach (var c in _children)
            {
                if (c.Item1.Id == child.Id)
                {
                    return c.Item2;
                }
            }

            return false;
        }

        public void SetAllChildrenOff()
        {
            for (var i = 0; i < _children.Count; ++i)
            {
                SetValue(i, false);
            }
        }

        public void SetAllChildrenOn()
        {
            for (var i = 0; i < _children.Count; ++i)
            {
                SetValue(i, true);
            }
        }
    }
}