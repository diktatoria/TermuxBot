using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TermuxBot.Common
{
    public class PluginController
    {
        public List<Plugin> _instanciatedPlugins;
        private Task _runningTask;

        public PluginController()
        {
            _instanciatedPlugins = new List<Plugin>();
        }

        public async Task InitializeAllPlugins(string pluginFolder)
        {
            // TODO: Get all dlls from Plugin Folder

            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var assembly = Assembly.LoadFile(Path.Combine(directory, "Plugin.PowerShellCLI.dll"));

            var type = assembly.GetType("Plugin.PowerShellCLI.PowerShellCLIPlugin");
            if(type == null) { return; }

            var powershellPlugin = Activator.CreateInstance(type, this) as Plugin;

            _runningTask = Task.Run(() => powershellPlugin.Initialize(CancellationToken.None))
                                .ContinueWith(OnPlugin_Exited);

            this.Initialized = true;
        }

        private void OnPlugin_Exited(Task obj)
        {
        }

        public bool Initialized { get; private set; }
    }
}
