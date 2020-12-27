using System;
using System.Threading;
using System.Threading.Tasks;

namespace TermuxBot.Common
{
    public abstract class Plugin
    {
        public Plugin(PluginController assignedController)
        {
            this.AssignedController = assignedController;
        }

        public abstract Task Initialize(CancellationToken cancellationToken);

        public abstract Task Unload();

        public PluginController AssignedController { get; }
    }
}
