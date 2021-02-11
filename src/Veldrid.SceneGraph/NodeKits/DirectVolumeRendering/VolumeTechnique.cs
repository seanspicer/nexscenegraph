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

using Veldrid.SceneGraph.RenderGraph;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph.NodeKits.DirectVolumeRendering
{
    public interface IVolumeTechnique : IObject
    {
        public IVolumeTile VolumeTile { get; internal set; }
        bool Valid();
        void Init();
        void Traverse(INodeVisitor nv);
    }
    
    public abstract class VolumeTechnique : Object, IVolumeTechnique
    {
        protected IVolumeTile _volumeTile;

        IVolumeTile IVolumeTechnique.VolumeTile
        {
            get => _volumeTile;
            set => _volumeTile = value;
        }

        public bool Valid()
        {
            throw new System.NotImplementedException();
        }

        public abstract void Init();

        public abstract void Update(IUpdateVisitor nv);
        
        public abstract void Cull(ICullVisitor nv);

        public virtual void Traverse(INodeVisitor nv)
        {
            if (null == _volumeTile) return;

            // if app traversal update the frame count.
            if (nv.Type == NodeVisitor.VisitorType.UpdateVisitor)
            {
                if (_volumeTile.Dirty) _volumeTile.Init();

                
                if (nv is IUpdateVisitor uv)
                {
                    Update(uv);
                    return;
                }

            }
            else if (nv.Type == NodeVisitor.VisitorType.CullVisitor)
            {
                if (nv is ICullVisitor cv)
                {
                    Cull(cv);
                    return;
                }
            }

            if (_volumeTile.Dirty) _volumeTile.Init();

            // otherwise fallback to the Group::traverse()
            _volumeTile.Traverse(nv);
        }
    }
}