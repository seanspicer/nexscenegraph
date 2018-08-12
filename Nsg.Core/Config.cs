using System;
using System.Composition;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Nsg.Core.Interfaces;

namespace Nsg.Core
{
    public sealed class Config
    {   
        private static readonly Lazy<Config> lazy =
            new Lazy<Config>(() => new Config());
        
        [Import]
        public IBackend Backend { get;  set; }
        
        public static Config Instance { get { return lazy.Value; } }

        private Config()
        {
            Compose();
        }
        
        private void Compose()
        {
            var executableLocation = Assembly.GetEntryAssembly().Location;
            var path = Path.GetDirectoryName(executableLocation);//Path.Combine(Path.GetDirectoryName(executableLocation), "Plugins");
            var assemblies = Directory
                .GetFiles(path, "*.dll", SearchOption.AllDirectories)
                .Select(AssemblyLoadContext.Default.LoadFromAssemblyPath)
                .ToList();
            var configuration = new ContainerConfiguration()
                .WithAssemblies(assemblies);
            using (var container = configuration.CreateContainer())
            {
                Backend = container.GetExport<IBackend>();
            }
        }
        
    }

}