//
// Copyright (c) 2018 Sean Spicer
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

using System;
using System.Runtime.InteropServices;

namespace Veldrid.SceneGraph
{
    /// <summary>
    /// Singleton class to capture information required for rendering
    /// </summary>
    public class DisplaySettings : IDisplaySettings
    {
        private static readonly Lazy<IDisplaySettings> lazy = new Lazy<IDisplaySettings>(() => new DisplaySettings());

        public static IDisplaySettings Instance => lazy.Value;

        public float ScreenWidth { get; set; }
        public float ScreenHeight { get; set; }
        public float ScreenDistance { get; set; }
        
        public GraphicsBackend GraphicsBackend { get; private set; }
        
        private DisplaySettings()
        {
            SetDefaults();
            ReadEnvironmentVariables();
        }

        private void ReadEnvironmentVariables()
        {
        }

        private void SetDefaults()
        {
            bool isMacOS = RuntimeInformation.OSDescription.Contains("Darwin");
            if (isMacOS)
            {
                GraphicsBackend = GraphicsBackend.Metal;
            }
            else
            {
                GraphicsBackend = GraphicsBackend.Vulkan;
            }
        }
    }
}