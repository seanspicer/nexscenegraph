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

using NUnit.Framework;

namespace Veldrid.SceneGraph.Tests
{
    public class TestNode : Node
    {
        public int AcceptCount { get; private set; } = 0;

        public void Reset()
        {
            AcceptCount = 0;
            
        }
        
        public override void Accept(INodeVisitor nv)
        {
            base.Accept(nv);
            AcceptCount++;
        }

        public override INode DeepCopy()
        {
            throw new System.NotImplementedException();
        }
    }
    
    public class TestNodeVisitor : NodeVisitor {}
    
    [TestFixture]
    public class NodeVisitorShould
    {
        [TestCase]
        public void PassCanaryTest()
        {
            Assert.That(4, Is.EqualTo(2 + 2));
        }

        [TestCase]
        public void TraverseSwitchCorrectly()
        {
            var testNode1 = new TestNode();
            var testNode2 = new TestNode();
            
            var switch1 = Switch.Create();
            switch1.AddChild(testNode1, true);
            switch1.AddChild(testNode2, false);

            var nv = new TestNodeVisitor();
            nv.TraversalMode = NodeVisitor.TraversalModeType.TraverseActiveChildren;
            
            switch1.Accept(nv);
            Assert.That(testNode1.AcceptCount, Is.EqualTo(1));
            Assert.That(testNode2.AcceptCount, Is.EqualTo(0));

            testNode1.Reset();
            testNode2.Reset();

            nv.TraversalMode = NodeVisitor.TraversalModeType.TraverseAllChildren;
            switch1.Accept(nv);
            Assert.That(testNode1.AcceptCount, Is.EqualTo(1));
            Assert.That(testNode2.AcceptCount, Is.EqualTo(1));
            
            testNode1.Reset();
            testNode2.Reset();

            nv.TraversalMode = NodeVisitor.TraversalModeType.TraverseNone;
            switch1.Accept(nv);
            Assert.That(testNode1.AcceptCount, Is.EqualTo(0));
            Assert.That(testNode2.AcceptCount, Is.EqualTo(0));
            
            testNode1.Reset();
            testNode2.Reset();

            nv.TraversalMode = NodeVisitor.TraversalModeType.TraverseParents;
            switch1.Accept(nv);
            Assert.That(testNode1.AcceptCount, Is.EqualTo(0));
            Assert.That(testNode2.AcceptCount, Is.EqualTo(0));
        }
    }
}