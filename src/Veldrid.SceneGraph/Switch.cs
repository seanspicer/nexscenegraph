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
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Veldrid.SceneGraph
{
    public class Switch : Group, ISwitch
    {
        private List<bool> _switchVals = new List<bool>();
        
        public new static ISwitch Create()
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

        internal void SetValue(int pos, bool value)
        {
            _children[pos] = Tuple.Create(_children[pos].Item1, value);
        }

        public bool GetValue(int pos, bool value)
        {
            return _children[pos].Item2;
        }

        internal void SetChildValue(INode child, bool value)
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

        internal void SetAllChildrenOff()
        {
            for (var i = 0; i < _children.Count; ++i)
            {
                SetValue(i, false);
            }
        }

        internal void SetAllChildrenOn()
        {
            for (var i = 0; i < _children.Count; ++i)
            {
                SetValue(i, true);
            }
        }
        
        public new IMutableSwitch GetMutable()
        {
            return new MutableSwitch(this);
        }
    }

    internal class MutableSwitch : MutableGroup, IMutableSwitch
    {
        private readonly Switch _switch;
        
        internal MutableSwitch(Switch switchNode) : base(switchNode)
        {
            _switch = switchNode;
        }

        public bool AddChild(INode child, bool value)
        {
            using (TimedLock.Lock(_switch))
            {
                return _switch.AddChild(child, value);
            }
        }

        public bool InsertChild(int index, INode child, bool visible)
        {
            using (TimedLock.Lock(_switch))
            {
                return _switch.InsertChild(index, child, visible);
            }
        }

        public void SetValue(int pos, bool value)
        {
            using (TimedLock.Lock(_switch))
            {
                _switch.SetValue(pos, value);
            }
        }

        public void SetChildValue(INode child, bool value)
        {
            using (TimedLock.Lock(_switch))
            {
                _switch.SetChildValue(child, value);
            }
        }

        public void SetAllChildrenOff()
        {
            using (TimedLock.Lock(_switch))
            {
                _switch.SetAllChildrenOff();
            }
        }

        public void SetAllChildrenOn()
        {
            using (TimedLock.Lock(_switch))
            {
                _switch.SetAllChildrenOn();
            }
        }
    }
}