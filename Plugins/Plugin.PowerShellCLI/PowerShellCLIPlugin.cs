using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.PowerShell;
using TermuxBot.Common;

namespace Plugin.PowerShellCLI
{
    public class PowerShellCLIPlugin : TermuxBot.Common.Plugin
    {
        public PowerShellCLIPlugin(PluginController assignedController)
            : base(assignedController)
        {
            
        }

        public override async Task Initialize(CancellationToken cancellationToken)
        {
            InitializeRunspaces(1, 10, new string[0]);

            RunScript("Write-Verbose \"TEST\"" + Environment.NewLine +
                      "Write \"Everything done. Exiting...\"");
            
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken);
            }
            
            cancellationToken.ThrowIfCancellationRequested();
        }

        
        /// <summary>
        /// Initialize the runspace pool.
        /// </summary>
        /// <param name="minRunspaces"></param>
        /// <param name="maxRunspaces"></param>
        public static void InitializeRunspaces(int minRunspaces, int maxRunspaces, string[] modulesToLoad)
        {
            // create the default session state.
            // session state can be used to set things like execution policy, language constraints, etc.
            // optionally load any modules (by name) that were supplied.
       
            var defaultSessionState = InitialSessionState.CreateDefault();
            defaultSessionState.ExecutionPolicy = Microsoft.PowerShell.ExecutionPolicy.Unrestricted;
       
            foreach (var moduleName in modulesToLoad)
            {
                defaultSessionState.ImportPSModule(moduleName);
            }
       
            // use the runspace factory to create a pool of runspaces
            // with a minimum and maximum number of runspaces to maintain.
       
            RsPool = RunspaceFactory.CreateRunspacePool(defaultSessionState);
            RsPool.SetMinRunspaces(minRunspaces);
            RsPool.SetMaxRunspaces(maxRunspaces);
       
            // set the pool options for thread use.
            // we can throw away or re-use the threads depending on the usage scenario.
       
            RsPool.ThreadOptions = PSThreadOptions.UseNewThread;
       
            // open the pool. 
            // this will start by initializing the minimum number of runspaces.
       
            RsPool.Open();
        }

        /// <summary>
        /// Runs a PowerShell script with parameters and prints the resulting pipeline objects to the console output. 
        /// </summary>
        /// <param name="scriptContents">The script file contents.</param>
        public static async Task RunScript(string scriptContents, Dictionary<string, object> scriptParameters = null)
        {
            if (RsPool == null)
            {
                throw new ApplicationException("Runspace Pool must be initialized before calling RunScript().");
            }
       
            // create a new hosted PowerShell instance using a custom runspace.
            // wrap in a using statement to ensure resources are cleaned up.
       
            using (PowerShell ps = PowerShell.Create())
            {
                // use the runspace pool.
                ps.RunspacePool = RsPool;
         
                // specify the script code to run.
                ps.AddScript(scriptContents);
         
                // specify the parameters to pass into the script.
                //ps.AddParameters(scriptParameters);
         
                // subscribe to events from some of the streams
                //ps.Streams.Error.DataAdded += Error_DataAdded;
                //ps.Streams.Warning.DataAdded += Warning_DataAdded;
                //ps.Streams.Information.DataAdded += Information_DataAdded;
         
                ps.Streams.Verbose.Completed += (sender, e) =>
                {
                    var streamObjectsReceived = sender as PSDataCollection<InformationRecord>;
                    var currentStreamRecord = streamObjectsReceived.LastOrDefault();
       
                    Console.WriteLine($"Verbose: {currentStreamRecord.MessageData}");
                };

                // execute the script and await the result.
                var pipelineObjects = await ps.InvokeAsync().ConfigureAwait(false);
         
                // print the resulting pipeline objects to the console.
                Console.WriteLine("----- Pipeline Output below this point -----");
                foreach (var item in pipelineObjects)
                {
                    Console.WriteLine(item.BaseObject.ToString());
                }
            }
        }
        
        /// <summary>
        /// The PowerShell runspace pool.
        /// </summary>
        private static RunspacePool RsPool { get; set; }

        public override Task Unload()
        {
            return Task.CompletedTask;
        }
    }
}
