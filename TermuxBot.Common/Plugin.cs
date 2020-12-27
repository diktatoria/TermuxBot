using System;
using System.Threading;
using System.Threading.Tasks;

namespace TermuxBot.Common
{
    public abstract class Plugin
    {
        public abstract Task Initialize(CancellationToken cancellationToken);

        public abstract Task Unload();
    }
}
