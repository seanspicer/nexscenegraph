//
// Copyright 2018-2021 Sean Spicer 
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

namespace Veldrid.SceneGraph
{
    public interface ISwitch : IGroup
    {
        bool AddChild(INode child, bool value);
        bool InsertChild(int index, INode child, bool visible);
        void SetValue(int pos, bool value);
        bool GetValue(int pos, bool value);
        void SetChildValue(INode child, bool value);
        bool GetChildValue(INode child, bool value);
        void SetAllChildrenOff();
        void SetAllChildrenOn();
    }

    public class Switch : Group, ISwitch
    {
        private List<bool> switchVals = new List<bool>();

        public override void Traverse(INodeVisitor nv)
        {
            foreach (var child in _children)
                if (child.Item2)
                    child.Item1.Accept(nv);
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
                if (_children[i].Item1.Id == child.Id)
                    SetValue(i, value);
        }

        public bool GetChildValue(INode child, bool value)
        {
            foreach (var c in _children)
                if (c.Item1.Id == child.Id)
                    return c.Item2;

            return false;
        }

        public void SetAllChildrenOff()
        {
            for (var i = 0; i < _children.Count; ++i) SetValue(i, false);
        }

        public void SetAllChildrenOn()
        {
            for (var i = 0; i < _children.Count; ++i) SetValue(i, true);
        }

        public new static ISwitch Create()
        {
            return new Switch();
        }
    }
}