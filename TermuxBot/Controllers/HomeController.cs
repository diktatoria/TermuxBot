using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TermuxBot.API;
using TermuxBot.Discord;
using TermuxBot.Models;

namespace TermuxBot.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private Task _deamonTask;
        private DiscordDæmon _discordDeamon;
        private PluginController _pluginController;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;

            // Init Plugins
            _pluginController = new PluginController(logger);

            // Init Discord Deamon
            _discordDeamon = new DiscordDæmon(logger, _pluginController);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Index()
        {
            if (!_pluginController.Initialized)
            {
                _pluginController.InitializeAllPlugins("Plugins").Wait();
            }

            if (!_discordDeamon.IsRunning)
            {
                try
                {
                    _deamonTask = _discordDeamon.InitializeAsync(CancellationToken.None)
                       .ContinueWith(OnDeamon_Exited);
                }
                catch (Exception e)
                {
                    _logger.Log(LogLevel.Error, e.Message);
                    _logger.Log(LogLevel.Warning, e.StackTrace);
                    throw;
                }
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        private void OnDeamon_Exited(Task obj)
        {
            _logger.Log(LogLevel.Critical, "Dicord Deamon exited");
        }
    }
}