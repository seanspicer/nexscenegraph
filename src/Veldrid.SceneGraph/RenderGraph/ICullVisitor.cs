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

namespace Veldrid.SceneGraph.RenderGraph
{
    public interface IMutableCullVisitor : IDisposable
    {
        void SetCurrentCamera(ICamera camera);
        
        void Reset();
        
        void Prepare();
    }
    
    public interface ICullVisitor : INodeVisitor
    {
        IRenderGroup OpaqueRenderGroup { get; set; }
        IRenderGroup TransparentRenderGroup { get; set; }
        GraphicsDevice GraphicsDevice { get; set; }
        ResourceFactory ResourceFactory { get; set; }
        ResourceLayout ResourceLayout { get; set; }
        int RenderElementCount { get; }
        

        ICamera GetCurrentCamera();
        Matrix4x4 GetModelViewMatrix();
        Matrix4x4 GetModelViewProjectionMatrix();
        Matrix4x4 GetModelViewInverseMatrix();
        Matrix4x4 GetModelViewProjectionInverseMatrix();
        Matrix4x4 GetProjectionMatrix();
        Vector3 GetEyeLocal();

        IMutableCullVisitor ToMutable();
    }
}