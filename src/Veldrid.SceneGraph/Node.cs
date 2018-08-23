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

namespace Veldrid.SceneGraph
{
    public class Node
    {
        public Guid Id { get; private set; }

        private Node Parent { get; set; }
        private List<Node> Children;

        public Node(Node parent = null)
        {
            Id = Guid.NewGuid();

            Parent = parent;
            Children = new List<Node>();
            
            parent?.Children.Add(this);
        }

        public void Add(Node node)
        {
            node.Parent = this;
            Children.Add(node);
        }

        internal virtual void Accept(DrawVisitor drawVisitor)
        {
            foreach (var child in Children)
            {
                child.Accept(drawVisitor);
            }
        }
    }
}