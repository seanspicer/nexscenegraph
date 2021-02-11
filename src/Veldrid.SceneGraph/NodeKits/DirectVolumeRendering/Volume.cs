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

using System.Collections.Generic;

namespace Veldrid.SceneGraph.NodeKits.DirectVolumeRendering
{
    public interface IVolume : IGroup
    {
        IVolumeTechnique VolumeTechniquePrototype { get; set; }

        IVolumeTile GetVolumeTile(VolumeTileId tileId);

        internal void UnRegisterVolumeTile(IVolumeTile volumeTile);
        internal void RegisterVolumeTile(IVolumeTile volumeTile);
    }
    
    public class Volume : Group, IVolume
    {
        public IVolumeTechnique VolumeTechniquePrototype { get; set; }

        protected Dictionary<VolumeTileId, IVolumeTile> VolumeTileDict { get; } =
            new Dictionary<VolumeTileId, IVolumeTile>();
        
        public new static IVolume Create()
        {
            return new Volume();
        }
        
        public IVolumeTile GetVolumeTile(VolumeTileId tileId)
        {
            return VolumeTileDict.TryGetValue(tileId, out var volumeTile) ? volumeTile : null;
        }

        void IVolume.UnRegisterVolumeTile(IVolumeTile volumeTile)
        {
            if (null == volumeTile) return;

            if (volumeTile.TileId.Valid())
            {
                VolumeTileDict.Remove(volumeTile.TileId);
            }
        }

        void IVolume.RegisterVolumeTile(IVolumeTile volumeTile)
        {
            if (null == volumeTile) return;

            if (volumeTile.TileId.Valid())
            {
                VolumeTileDict[volumeTile.TileId] = volumeTile;
            }
        }

        internal void DirtyRegisteredVolumeTiles()
        {
            foreach (var tile in VolumeTileDict.Values)
            {
                tile.SetDirty(true);
            }
        }
    }
}