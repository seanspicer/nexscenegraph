//
// Copyright 2018 Sean Spicer 
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
    public class KtxFileProcessor : BinaryAssetProcessor<byte[]>
    {
        public override byte[] ProcessT(Stream stream, string extension)
        {
            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();

            //KtxFile ktx = KtxFile.Load(stream, false);

            //uint width = ktx.Header.PixelWidth;
            //uint height = ktx.Header.PixelHeight;
            //if (height == 0) height = width;

            //uint arrayLayers = Math.Max(1, ktx.Header.NumberOfArrayElements);
            //uint mipLevels = Math.Max(1, ktx.Header.NumberOfMipmapLevels);

            //// Copy texture data into single array
            //List<byte> bytes = new List<byte>();
            //for (uint level = 0; level < mipLevels; level++)
            //{
            //    KtxMipmapLevel mipmap = ktx.Mipmaps[level];
            //    for (uint layer = 0; layer < arrayLayers; layer++)
            //    {
            //        KtxArrayElement ktxLayer = mipmap.ArrayElements[layer];
            //        Debug.Assert(ktxLayer.Faces.Length == 1);
            //        byte[] pixelData = ktxLayer.Faces[0].Data;
            //        bytes.AddRange(pixelData);
            //    }
            //}

            //return new ProcessedTexture(
            //    PixelFormat.BC3_UNorm, // TODO translate KtxFile.Header.GlFormat
            //    TextureType.Texture2D,
            //    width,
            //    height,
            //    1,
            //    mipLevels,
            //    arrayLayers,
            //    bytes.ToArray());
        }
    }
}
