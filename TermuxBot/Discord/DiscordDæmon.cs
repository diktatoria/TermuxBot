using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.Extensions.Logging;
using TermuxBot.Controllers;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace TermuxBot.Discord
{
    public class DiscordDæmon
    {
        private ILogger<HomeController> _logger;

        public DiscordDæmon(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            _logger.Log(LogLevel.Information, "Starting Dicord Deamon...");

            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = "NzkyMzcxNDE0MTY5OTQ0MDk1.X-cvYg.WWUB7DI2LOF0-wRS8VCjcmaw270",
                TokenType = TokenType.Bot       
            });

            discord.MessageCreated += async (e) =>
            {
                if (e.Message.Content.ToLower().StartsWith("ping")) 
                    await e.Message.RespondAsync("pong!");
            };

            await discord.ConnectAsync();
            //await Task.Delay(-1);
        }
    }

}
