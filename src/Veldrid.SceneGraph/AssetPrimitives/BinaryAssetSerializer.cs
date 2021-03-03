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

using System.IO;

namespace Veldrid.SceneGraph.AssetPrimitives
{
    public abstract class BinaryAssetSerializer
    {
        public abstract object Read(BinaryReader reader);
        public abstract void Write(BinaryWriter writer, object value);
    }

    public abstract class BinaryAssetSerializer<T> : BinaryAssetSerializer
    {
        public override void Write(BinaryWriter writer, object value)
        {
            WriteT(writer, (T) value);
        }

        public override object Read(BinaryReader reader)
        {
            return ReadT(reader);
        }

        public abstract T ReadT(BinaryReader reader);
        public abstract void WriteT(BinaryWriter writer, T value);
    }
}