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
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Veldrid.SceneGraph
{
    public interface IDisplaySettings
    {
        uint ScreenWidth { get; }

        uint ScreenHeight { get; }

        float ScreenDistance { get; }

        GraphicsBackend GraphicsBackend { get; }
        void SetScreenWidth(uint width);
        void SetScreenHeight(uint height);
        void SetScreenDistance(float distance);
    }

    /// <summary>
    ///     Singleton class to capture information required for rendering
    /// </summary>
    public class DisplaySettings : IDisplaySettings
    {
        private static readonly Dictionary<IView, DisplaySettings> _displaySettingsCache =
            new Dictionary<IView, DisplaySettings>();

        private DisplaySettings()
        {
            SetDefaults();
            ReadEnvironmentVariables();
        }

        public uint ScreenWidth { get; private set; }

        public void SetScreenWidth(uint width)
        {
            ScreenWidth = width;
        }

        public uint ScreenHeight { get; private set; }

        public void SetScreenHeight(uint height)
        {
            ScreenHeight = height;
        }

        public float ScreenDistance { get; private set; }

        public void SetScreenDistance(float distance)
        {
            ScreenDistance = distance;
        }

        public GraphicsBackend GraphicsBackend { get; private set; }
        //private static readonly Lazy<IDisplaySettings> lazy = new Lazy<IDisplaySettings>(() => new DisplaySettings());

        public static IDisplaySettings Instance(IView view)
        {
            if (null == view) throw new ArgumentNullException("view cannot be null");

            if (_displaySettingsCache.TryGetValue(view, out var displaySettings)) return displaySettings;

            displaySettings = new DisplaySettings();
            _displaySettingsCache.Add(view, displaySettings);
            return displaySettings;
        }

        private void ReadEnvironmentVariables()
        {
        }

        private void SetDefaults()
        {
            //SetScreenWidth(0.325f);
            //SetScreenHeight(0.26f);
            //SetScreenDistance(0.5f);

            var isMacOS = RuntimeInformation.OSDescription.Contains("Darwin");
            if (isMacOS)
                GraphicsBackend = GraphicsBackend.Metal;
            else
                GraphicsBackend = GraphicsBackend.Direct3D11;
        }
    }
}