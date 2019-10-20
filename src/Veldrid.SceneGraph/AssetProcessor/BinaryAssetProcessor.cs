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

namespace Veldrid.SceneGraph.AssetProcessor
{
    public abstract class BinaryAssetProcessor
    {
        public abstract object Process(Stream stream, string extension);
    }

    public abstract class BinaryAssetProcessor<T> : BinaryAssetProcessor
    {
        public override object Process(Stream stream, string extension) => ProcessT(stream, extension);

        public abstract T ProcessT(Stream stream, string extension);
    }
}
