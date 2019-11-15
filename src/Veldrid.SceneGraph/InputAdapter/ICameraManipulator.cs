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
using System.Numerics;
using System.Reactive.Concurrency;

namespace Veldrid.SceneGraph.InputAdapter
{
    public interface ICameraManipulator : IInputEventHandler
    {
        void SetNode(INode node);

        INode GetNode();
        
        void ViewAll();

        void ComputeHomePosition(ICamera camera=null, bool useBoundingBox=false);

        void SetHomePosition(Vector3 eye, Vector3 center, Vector3 up, bool autoComputeHomePosition=false);
        
        void GetHomePosition(out Vector3 eye, out Vector3 center, out Vector3 up);

        void SetAutoComputeHomePosition(bool flag);

        bool GetAutoComputeHomePosition();
    }
}