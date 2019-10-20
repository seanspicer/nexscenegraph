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

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Veldrid.SceneGraph.AssetPrimitives
{
    public static class BinaryExtensions
    {
        public static unsafe T ReadEnum<T>(this BinaryReader reader)
        {
            int i32 = reader.ReadInt32();
            return Unsafe.Read<T>(&i32);
        }

        public static void WriteEnum<T>(this BinaryWriter writer, T value)
        {
            int i32 = Convert.ToInt32(value);
            writer.Write(i32);
        }

        public static byte[] ReadByteArray(this BinaryReader reader)
        {
            int byteCount = reader.ReadInt32();
            return reader.ReadBytes(byteCount);
        }

        public static void WriteByteArray(this BinaryWriter writer, byte[] array)
        {
            writer.Write(array.Length);
            writer.Write(array);
        }

        public static void WriteObjectArray<T>(this BinaryWriter writer, T[] array, Action<BinaryWriter, T> writeFunc)
        {
            writer.Write(array.Length);
            foreach (T item in array)
            {
                writeFunc(writer, item);
            }
        }

        public static T[] ReadObjectArray<T>(this BinaryReader reader, Func<BinaryReader, T> readFunc)
        {
            int length = reader.ReadInt32();
            T[] ret = new T[length];
            for (int i = 0; i < length; i++)
            {
                ret[i] = readFunc(reader);
            }

            return ret;
        }

        public static unsafe void WriteBlittableArray<T>(this BinaryWriter writer, T[] array)
        {
            int sizeofT = Unsafe.SizeOf<T>();
            int totalBytes = array.Length * sizeofT;

            GCHandle handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            byte* ptr = (byte*)handle.AddrOfPinnedObject();

            writer.Write(array.Length);
            for (int i = 0; i < totalBytes; i++)
            {
                writer.Write(ptr[i]);
            }

            handle.Free();
        }

        public static unsafe T[] ReadBlittableArray<T>(this BinaryReader reader)
        {
            int sizeofT = Unsafe.SizeOf<T>();
            int length = reader.ReadInt32();
            T[] ret = new T[length];
            GCHandle handle = GCHandle.Alloc(ret, GCHandleType.Pinned);

            int totalBytes = length * sizeofT;
            byte* ptr = (byte*)handle.AddrOfPinnedObject();
            for (int i = 0; i < totalBytes; i++)
            {
                ptr[i] = reader.ReadByte();
            }

            handle.Free();

            return ret;
        }
    }
}
