using System;
using System.Management.Automation.Runspaces;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.PowerShell;

namespace Plugin.PowerShellCLI
{
    public class PowerShellCLIPlugin : TermuxBot.Common.Plugin
    {
        public PowerShellCLIPlugin()
        {
            
        }

        public override async Task Initialize(CancellationToken cancellationToken)
        {
            var sessionState = InitialSessionState.CreateDefault();

            int result = ConsoleShell.Start(sessionState, "TermuX Bot 1.0", "Enter 'PS help' for help", new string[] {});
            if(result != 0) { return; }

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken);
            }
            
            cancellationToken.ThrowIfCancellationRequested();
        }

        public override Task Unload()
        {
            return Task.CompletedTask;
        }
    }
}
