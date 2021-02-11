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

using System.Numerics;
using Veldrid.SceneGraph.Math.IsoSurface;

namespace Veldrid.SceneGraph.NodeKits.DirectVolumeRendering
{
    public interface ILayer : IObject
    {
        ILocator Locator { get; set; }
        void Update(INodeVisitor nv);
        
        // Specify when an volume layer (Image) requires update traversal.
        bool RequiresUpdateTraversal();
    }
    
    public class Layer : Object, ILayer
    {
        public ILocator Locator { get; set; }
        public virtual void Update(INodeVisitor nv)
        {
        }

        public virtual bool RequiresUpdateTraversal()
        {
            return false;
        }
    }

    public interface IVoxelVolumeLayer : ILayer
    {
        IVoxelVolume VoxelVolume { get; }
    }

    public class VoxelVolumeLayer : Layer, IVoxelVolumeLayer
    {
        public IVoxelVolume VoxelVolume { get; }
        
        public static IVoxelVolumeLayer Create(IVoxelVolume voxelVolume)
        {
            return new VoxelVolumeLayer(voxelVolume);
        }

        protected VoxelVolumeLayer(IVoxelVolume voxelVolume)
        {
            VoxelVolume = voxelVolume;
            BuildLocator();
        }

        protected void BuildLocator()
        {
            Locator = LevoyCabralLocator.Create(VoxelVolume);
        }
    }
    
    
}