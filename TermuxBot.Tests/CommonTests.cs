using System;
using System.Threading;
using System.Threading.Tasks;
using Plugin.PowerShellCLI;
using Xunit;

namespace TermuxBot.Tests
{
    public class CommonTests
    {
        [Fact]
        public async Task TestPowershellPlugin()
        {
            PowerShellCLIPlugin plugin = new();
            await plugin.Initialize(new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token);

        }
    }
}
