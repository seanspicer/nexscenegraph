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
using System.Threading;

namespace Veldrid.SceneGraph
{
    public class CrossThreadException : Exception
    {
        public CrossThreadException(string message) : base(message)
        {
        }
    }
    
    public interface IThreadInfo
    {
        int RenderingThreadId { get; }
        public void SetRenderingThreadCurrent();

        public void AssertRenderingThread();
    }
    
    public class ThreadInfo : IThreadInfo
    {
        public int RenderingThreadId { get; private set; }


        private static readonly Lazy<IThreadInfo> Lazy =
            new Lazy<IThreadInfo>(() => new ThreadInfo());

        private ThreadInfo()
        {
            RenderingThreadId = -42;
        }

        public static IThreadInfo Instance => Lazy.Value;
        
        public void SetRenderingThreadCurrent()
        {
            RenderingThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        public void AssertRenderingThread()
        {
            if (RenderingThreadId == -42) return;
            
            var currentThread = Thread.CurrentThread.ManagedThreadId;
            if (currentThread != RenderingThreadId)
            {
                throw new CrossThreadException($"InsertChild called from a thread (ThreadId:{currentThread}) that is not the rendering thread (ThreadId: {RenderingThreadId}).  This is currently not valid");
            }
        }
    }
}