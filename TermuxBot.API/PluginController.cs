﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TermuxBot.API
{
    public class PluginController : IPluginController
    {
        private PluginAssemblyLoadContext _pluginLoadContext;
        private Task _runningTask;

        public PluginController(ILogger logger)
        {
            this.InstanciatedPlugins = new List<Plugin>();

            this.Logger = logger;
        }

        public async Task InitializeAllPlugins(string pluginFolder)
        {
            // string directory = Path.Combine(Environment.CurrentDirectory, pluginFolder);
            string directory = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), pluginFolder);

            _pluginLoadContext = new PluginAssemblyLoadContext("Plugin Context", directory);
            Assembly assembly = _pluginLoadContext.LoadFromAssemblyPath(Path.Combine(directory, "Plugin.PowerShellCLI.dll"));

            // TODO: Query all types derived from Plugin
            Type type = assembly.GetType("Plugin.PowerShellCLI.PowerShellCLIPlugin");
            if (type == null) { return; }

            //var constructors = type.GetConstructors();
            //var relevantConstructor = constructors.LastOrDefault();

            //if (relevantConstructor == null)
            //{
            //    this.Logger.Log(LogLevel.Error, $"Unable to load plugin '{"Plugin.PowerShellCLI.PowerShellCLIPlugin"}'");
            //    return;
            //}

            Plugin powerShellPlugin = Activator.CreateInstance(type, this.Logger) as Plugin;
            this.InstanciatedPlugins.Add(powerShellPlugin);

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

        public async Task Invoke(string command, TextWriter outputStream)
        {
            foreach (Plugin instanciatedPlugin in this.InstanciatedPlugins)
            {
                await instanciatedPlugin.Invoke(command, outputStream);
            }
        }

        private void OnPlugin_Exited(Task obj)
        {
        }

        public bool Initialized { get; private set; }
        public List<Plugin> InstanciatedPlugins { get; }
        public ILogger Logger { get; }
    }
}