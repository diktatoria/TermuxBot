using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TermuxBot.API
{
    public abstract class Plugin
    {
        public Plugin(ILogger logger)
        {
            this.Logger = logger;
        }

        public abstract Task Initialize(CancellationToken cancellationToken);

        public abstract Task Invoke(string command, TextWriter outputStream);

        public abstract Task Unload();

        public ILogger Logger { get; }
    }
}