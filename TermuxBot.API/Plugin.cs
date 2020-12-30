using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TermuxBot.API
{
    public abstract class Plugin
    {
        public Plugin(ILogger<Controller> logger)
        {
            this.Logger = logger;
        }

        public abstract Task Initialize(CancellationToken cancellationToken);

        public abstract Task Unload();

        public ILogger<Controller> Logger { get; }
    }
}