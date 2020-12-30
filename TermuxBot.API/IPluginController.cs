using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TermuxBot.API
{
    public interface IPluginController
    {
        public Task Invoke(string command, TextWriter outputStream);

        public List<Plugin> InstanciatedPlugins { get; }

        public ILogger<Controller> Logger { get; }
    }
}