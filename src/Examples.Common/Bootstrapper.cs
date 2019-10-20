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

using Serilog;
using SharpDX.Win32;
using Veldrid.SceneGraph.Logging;

namespace Examples.Common
{
    public static class Bootstrapper
    {
        public static void Configure()
        {
            BuildLogger();
        }

        private static void BuildLogger()
        {
//            Log.Logger = new LoggerConfiguration()
//                .Enrich.FromLogContext()
//                .MinimumLevel.Information()
//                .WriteTo.ColoredConsole(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm::ss} [{Level}]: {Message}{NewLine}")
//                .CreateLogger();
//
//            // Assign to Common.Logging adapter.
//            LogManager.Adapter = new SerilogFactoryAdapter(Log.Logger);
            
        }
    }
}