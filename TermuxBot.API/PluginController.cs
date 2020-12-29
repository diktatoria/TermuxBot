using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TermuxBot.API
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

            try
            {
                string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                Assembly assembly = Assembly.LoadFile(Path.Combine(directory, "Plugin.PowerShellCLI.dll"));

                Type type = assembly.GetType("Plugin.PowerShellCLI.PowerShellCLIPlugin");
                if (type == null) { return; }

                Plugin powerShellPlugin = await Task.Run(() => Activator.CreateInstance(type, this) as Plugin);

                this.Initialized = true;

                _runningTask = Task.Run(() => powerShellPlugin.Initialize(CancellationToken.None))
                    .ContinueWith(task =>
                    {
                        if (task.IsCompletedSuccessfully)
                        {
                            OnPlugin_Exited(task);
                            return;
                        }

                        this.Logger.Log(LogLevel.Critical, "Unable to initialize plugin 'Plugin.PowerShellCLI.PowerShellCLIPlugin'");
                        this.Logger.Log(LogLevel.Error, task.Exception?.StackTrace);
                    });
            }
            catch (Exception)
            {
                this.Logger.Log(LogLevel.Critical, "Unable to initialize plugins!");
                throw;
            }
        }

        private void OnPlugin_Exited(Task obj)
        {
        }

        public bool Initialized { get; private set; }

        public List<Plugin> InstanciatedPlugins { get; }
        public ILogger<Controller> Logger { get; }
    }
}