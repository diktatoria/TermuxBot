using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.PowerShell;
using TermuxBot.API;

namespace Plugin.PowerShellCLI
{
    /// <summary>
    /// A plugin for PowerShell execution
    /// </summary>
    /// <seealso cref="TermuxBot.Common.Plugin" />
    public class PowerShellCLIPlugin : TermuxBot.API.Plugin
    {
        private TextWriter _currentOutputStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="PowerShellCLIPlugin" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public PowerShellCLIPlugin(ILogger logger)
            : base(logger)
        {
            logger.LogInformation("Creating Plugin Plugin.PowerShellCLI.PowerShellCLIPlugin");
        }

        /// <summary>
        /// Initializes the specified cancellation token.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public override async Task Initialize(CancellationToken cancellationToken)
        {
            this.InitializeRunspaces(1, 10, new string[] { "PSReadline" });

            // string scriptData = await File.ReadAllTextAsync("./TermuxInitializeScript.ps1", cancellationToken);
            // Task task = this.RunScript(scriptData);

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken);
            }

            //await task;

            cancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Initialize the runspace pool.
        /// </summary>
        /// <param name="minRunspaces">The minimum runspaces.</param>
        /// <param name="maxRunspaces">The maximum runspaces.</param>
        /// <param name="modulesToLoad">The modules to load.</param>
        public void InitializeRunspaces(int minRunspaces, int maxRunspaces, string[] modulesToLoad)
        {
            // create the default session state.
            // session state can be used to set things like execution policy, language constraints, etc.
            // optionally load any modules (by name) that were supplied.

            var defaultSessionState = InitialSessionState.CreateDefault();
            defaultSessionState.ExecutionPolicy = ExecutionPolicy.Unrestricted;

            foreach (string moduleName in modulesToLoad)
            {
                defaultSessionState.ImportPSModule(moduleName);
            }

            // use the runspace factory to create a pool of runspaces
            // with a minimum and maximum number of runspaces to maintain.

            //this.RsPool = RunspaceFactory.CreateRunspacePool(defaultSessionState);
            //this.RsPool.SetMinRunspaces(minRunspaces);
            //this.RsPool.SetMaxRunspaces(maxRunspaces);

            // set the pool options for thread use.
            // we can throw away or re-use the threads depending on the usage scenario.

            //this.RsPool.ThreadOptions = PSThreadOptions.UseNewThread;

            // open the pool.
            // this will start by initializing the minimum number of runspaces.

            // this.RsPool.Open();
        }

        public override async Task Invoke(string command, TextWriter outputStream)
        {
            if (!command.StartsWith("ps ")) { return; }

            string truncatedCommand = command.Remove(0, 3);

            await this.RunScript(truncatedCommand, null, outputStream);

            //if (truncatedCommand == "get-help")
            //{
            //    await outputStream.WriteAsync("No help available.");
            //}
            //else
            //{
            //    await outputStream.WriteAsync("Command not found");
            //}
        }

        /// <summary>
        /// Runs a PowerShell script with parameters and prints the resulting pipeline objects to the console output.
        /// </summary>
        /// <param name="scriptContents">The script file contents.</param>
        /// <param name="scriptParameters">The script parameters.</param>
        /// <exception cref="ApplicationException">Runspace Pool must be initialized before calling RunScript().</exception>
        public async Task RunScript(string scriptContents, Dictionary<string, object> scriptParameters = null, TextWriter outputStream = null)
        {
            _currentOutputStream = outputStream;

            try
            {
                if (this.RsPool == null)
                {
                    throw new ApplicationException("Runspace Pool must be initialized before calling RunScript().");
                }

                // create a new hosted PowerShell instance using a custom runspace.
                // wrap in a using statement to ensure resources are cleaned up.

                using var ps = PowerShell.Create();

                // use the runspace pool.
                // ps.RunspacePool = this.RsPool;

                // specify the script code to run.
                ps.AddScript(scriptContents);

                // specify the parameters to pass into the script.
                if (scriptParameters != null)
                {
                    ps.AddParameters(scriptParameters);
                }

                // subscribe to events from some of the streams
                ps.Streams.Error.DataAdded += this.OnScriptError_DataAdded;
                ps.Streams.Warning.DataAdded += this.OnScriptWarning_DataAdded;
                ps.Streams.Information.DataAdded += this.OnScriptInformation_DataAdded;

                ps.Streams.Verbose.Completed += this.OnScript_Completed;

                // execute the script and await the result.
                PSDataCollection<PSObject> pipelineObjects = await ps.InvokeAsync().ConfigureAwait(false);

                // print the resulting pipeline objects to the console.
                // _currentOutputStream.WriteLine("----- Pipeline Output below this point -----");
                foreach (PSObject item in pipelineObjects)
                {
                    _currentOutputStream.WriteLine(item.BaseObject.ToString());
                }
            }
            catch (Exception e)
            {
                _currentOutputStream.Write("EXCEPTION: " + e.Message);
            }
            finally
            {
                _currentOutputStream = null;
            }
        }

        /// <summary>
        /// Unloads this instance.
        /// </summary>
        /// <returns></returns>
        public override Task Unload()
        {
            return Task.CompletedTask;
        }

        private void OnScript_Completed(object sender, EventArgs e)
        {
            var streamObjectsReceived = sender as PSDataCollection<InformationRecord>;
            InformationRecord currentStreamRecord = streamObjectsReceived.LastOrDefault();

            this.Logger?.Log(LogLevel.Trace, $"TermuxBot-PowerShell: {currentStreamRecord.MessageData}");
            _currentOutputStream.Write(currentStreamRecord.MessageData);
        }

        /// <summary>
        /// Called when [script error data added].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DataAddedEventArgs"/> instance containing the event data.</param>
        private void OnScriptError_DataAdded(object sender, DataAddedEventArgs e)
        {
            var streamObjectsReceived = sender as PSDataCollection<ErrorRecord>;
            ErrorRecord currentStreamRecord = streamObjectsReceived.LastOrDefault();

            this.Logger?.Log(LogLevel.Error, $"{currentStreamRecord.Exception.Message}");
            _currentOutputStream.Write("ERROR: " + currentStreamRecord.Exception.Message);
        }

        /// <summary>
        /// Called when [script information data added].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DataAddedEventArgs"/> instance containing the event data.</param>
        private void OnScriptInformation_DataAdded(object sender, DataAddedEventArgs e)
        {
            var streamObjectsReceived = sender as PSDataCollection<InformationRecord>;
            InformationRecord currentStreamRecord = streamObjectsReceived.LastOrDefault();

            this.Logger?.Log(LogLevel.Information, $"{currentStreamRecord.MessageData}");
            _currentOutputStream.Write("INFO: " + currentStreamRecord.MessageData);
        }

        /// <summary>
        /// Called when [script warning data added].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DataAddedEventArgs"/> instance containing the event data.</param>
        private void OnScriptWarning_DataAdded(object sender, DataAddedEventArgs e)
        {
            var streamObjectsReceived = sender as PSDataCollection<WarningRecord>;
            WarningRecord currentStreamRecord = streamObjectsReceived.LastOrDefault();

            this.Logger?.Log(LogLevel.Warning, $"{currentStreamRecord.Message}");
            _currentOutputStream.Write("WARNING: " + currentStreamRecord.Message);
        }

        /// <summary>
        /// The PowerShell runspace pool.
        /// </summary>
        private RunspacePool RsPool { get; set; }
    }
}