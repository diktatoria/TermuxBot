using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TermuxBot.Common;
using TermuxBot.Discord;
using TermuxBot.Models;

namespace TermuxBot.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private DiscordDæmon _discordDeamon;
        private PluginController _pluginController;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _discordDeamon = new DiscordDæmon(logger);
            
            // Init Plugins
            _pluginController = new PluginController();

            // Init Discord Deamon
            _logger.Log(LogLevel.Information, "Starting Dicord Deamon...");
            _discordDeamon.InitializeAsync(CancellationToken.None)
                .ContinueWith(OnDeamon_Exited);
        }

        private void OnDeamon_Exited(Task obj)
        {
            _logger.Log(LogLevel.Critical, "Dicord Deamon exited");
        }

        public IActionResult Index()
        {
            if (!_pluginController.Initialized)
            {
                _pluginController.InitializeAllPlugins("./").Wait();
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
