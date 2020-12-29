using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TermuxBot.Common
{
    public class PluginController
    {
        private Task _runningTask;

        public PluginController(ILogger<Controller> logger)
        {
            this.InstanciatedPlugins = new List<Plugin>();

            this.Logger = logger;
        }

        public async Task InitializeAllPlugins(string pluginFolder)
        {
            // TODO: Get all dlls from Plugin Folder

            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Assembly assembly = Assembly.LoadFile(Path.Combine(directory, "Plugin.PowerShellCLI.dll"));

            Type type = assembly.GetType("Plugin.PowerShellCLI.PowerShellCLIPlugin");
            if (type == null) { return; }

            var powerShellPlugin = Activator.CreateInstance(type, this) as Plugin;

            _runningTask = Task.Run(() => powerShellPlugin.Initialize(CancellationToken.None))
                                .ContinueWith(OnPlugin_Exited);

            this.Initialized = true;
        }

        private void OnPlugin_Exited(Task obj)
        {
        }

        public bool Initialized { get; private set; }

        public List<Plugin> InstanciatedPlugins { get; }
        public ILogger<Controller> Logger { get; }
    }
}