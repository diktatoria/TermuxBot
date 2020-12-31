using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.PowerShell.Commands;
using Plugin.PowerShellCLI;
using TermuxBot.API;
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

            controller.InitializeAllPlugins("");
        }
    }
}