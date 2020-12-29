using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.PowerShell.Commands;
using Plugin.PowerShellCLI;
using TermuxBot.Common;
using Xunit;

namespace TermuxBot.Tests
{
    public class CommonTests
    {
        [Fact]
        public async Task TestPowerShellPlugin()
        {
            LoggerFactory factory = new LoggerFactory();
            Logger<Controller> ctrl = new Logger<Controller>(factory);
            PluginController controller = new PluginController(ctrl);

            PowerShellCLIPlugin plugin = new(controller);
            await plugin.Initialize(new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token);
        }
    }
}