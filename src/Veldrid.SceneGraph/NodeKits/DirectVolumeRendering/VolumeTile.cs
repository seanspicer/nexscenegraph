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
using System.Linq;
using System.Net.Security;
using System.Numerics;
using Newtonsoft.Json.Bson;

namespace Veldrid.SceneGraph.NodeKits.DirectVolumeRendering
{

    public class VolumeTileId
    {
        public int Level { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Z { get; private set; }
        
        public VolumeTileId(int level, int x, int y, int z)
        {
            Level = level;
            X = x;
            Y = y;
            Z = z;
        }

        public int CompareTo(VolumeTileId other)
        {
            if (Level == other.Level &&
                X == other.X &&
                Y == other.Y &&
                Z == other.Z)
            {
                return 0;
            }

            if (Level < other.Level) return -1;
            if (Level > other.Level) return 1;
            if (X < other.X) return -1;
            if (X > other.X) return 1;
            if (Y < other.Y) return -1;
            if (Y > other.Y) return 1;
            if (Z < other.Z) return -1;
            return 1;
        }

        public bool Valid()
        {
            return Level >= 0;
        }

    }

    public interface IVolumeTile : IGroup
    {
        IVolume Volume { get; }
        void SetVolume(IVolume volume);

        VolumeTileId TileId { get; }
        void SetTileId(VolumeTileId tileId);

        IVolumeTechnique VolumeTechnique { get; }
        void SetVolumeTechnique(IVolumeTechnique volumeTechnique);
        
        ILocator Locator { get; set; }
        ILayer Layer { get; set; }
        
        bool Dirty { get; }
        void SetDirty(bool dirty);

        void Init();

        void TraverseGroup(INodeVisitor nv);
    }

    public class VolumeTile : Group, IVolumeTile
    {
        public ILocator Locator { get; set; }
        
        public ILayer Layer { get; set; }
        
        public VolumeTileId TileId { get; protected set; }
        
        public IVolume Volume { get; protected set; } 
        
        public IVolumeTechnique VolumeTechnique { get; protected set; }
        
        public bool Dirty { get; protected set; }

        public new static IVolumeTile Create()
        {
            return new VolumeTile();
        }

        protected VolumeTile()
        {
            Volume = null;
            Dirty = false;
            _hasBeenTraversal = false;
        }
        
        public void SetVolume(IVolume volume)
        {
            if (Volume == volume) return;

            Volume?.UnRegisterVolumeTile(this);

            Volume = volume;

            Volume?.RegisterVolumeTile(this);

        }
        
        public void SetTileId(VolumeTileId tileId)
        {
            if (TileId.Equals(tileId)) return;

            Volume?.UnRegisterVolumeTile(this);

            TileId = tileId;

            Volume?.RegisterVolumeTile(this);
        }

        public void SetVolumeTechnique(IVolumeTechnique volumeTechnique)
        {
            if (VolumeTechnique == volumeTechnique) return;

            int dirtyDelta = Dirty ? -1 : 0;

            if (null != VolumeTechnique)
            {
                VolumeTechnique.VolumeTile = null;
            }

            VolumeTechnique = volumeTechnique;

            if (null != VolumeTechnique)
            {
                VolumeTechnique.VolumeTile = this;
                ++dirtyDelta;
            }

            if (dirtyDelta>0) SetDirty(true);
            else if (dirtyDelta<0) SetDirty(false);
        }

        public void SetDirty(bool dirty)
        {
            if (Dirty==dirty) return;

            Dirty = dirty;

            if (Dirty)
            {
                SetNumChildrenRequiringUpdateTraversal(GetNumChildrenRequiringUpdateTraversal()+1);
            }
            else if (GetNumChildrenRequiringUpdateTraversal()>0)
            {
                SetNumChildrenRequiringUpdateTraversal(GetNumChildrenRequiringUpdateTraversal()-1);
            }
        }

        public void Init()
        {
            if (null != VolumeTechnique && Dirty)
            {
                VolumeTechnique.Init();

                SetDirty(false);
            }
        }

        private bool _hasBeenTraversal = false;

        public void TraverseGroup(INodeVisitor nv)
        {
            base.Traverse(nv);
        }
        
        public override void Traverse(INodeVisitor nv)
        {
            if (!_hasBeenTraversal)
            {
                if (null != Volume)
                {
                    NodePath nodePath = nv.NodePath;
                    if (nodePath.Any())
                    {
                        foreach (var node in nodePath.Reverse())
                        {
                            if (node is IVolume volume)
                            {
                                SetVolume(volume);
                            }
                        }
                    }
                }

                _hasBeenTraversal = true;
            }
            
            if (nv.Type==NodeVisitor.VisitorType.UpdateVisitor &&
                Layer.RequiresUpdateTraversal())
            {
                Layer.Update(nv);
            }

            if (null != VolumeTechnique)
            {
                VolumeTechnique.Traverse(nv);
            }
            else
            {
                base.Traverse(nv);
            }
        }

        public override IBoundingSphere ComputeBound()
        {
            var masterLocator = Locator;
            if (null != Layer && null != masterLocator)
            {
                masterLocator = Layer.Locator;
            }

            if (null != masterLocator)
            {
                Vector3 left = Vector3.Zero;
                Vector3 right = Vector3.Zero;
                masterLocator.ComputeLocalBounds(ref left, ref right);

                return BoundingSphere.Create((left+right)*0.5f, (right-left).Length()*0.5f);
            }
            else if (null != Layer)
            {
                // we have a layer but no Locator defined so will assume a Identity Locator
                return BoundingSphere.Create( new Vector3(0.5f,0.5f,0.5f), 0.867f);
            }
            else
            {
                return BoundingSphere.Create();
            }
        }
    }
}