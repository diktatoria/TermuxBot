using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.Log(LogLevel.Information, "Starting Dicord Deamon...");

                // Read Token from file
                string token = String.Empty;
                if (File.Exists("./discord.token"))
                {
                    token = await File.ReadAllTextAsync("./discord.token", cancellationToken);
                }
                else
                {
                    File.Create("./discord.token");
                    throw new FileNotFoundException($"Token in File '{Path.Combine(Environment.CurrentDirectory, "discord.token")}' not found! ");
                }

                var discord = new DiscordClient(new DiscordConfiguration()
                {
                    Token = token,
                    TokenType = TokenType.Bot
                });

                discord.MessageCreated += async (e) =>
                {
                    if (e.Message.Content.ToLower().StartsWith("ping"))
                        await e.Message.RespondAsync("pong!");
                };

                await discord.ConnectAsync();

                this.IsRunning = true;

                await Task.Delay(-1, cancellationToken);
                this.IsRunning = false;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Critical, $"Discord Dæmon exited: {e.Message}");
                _logger.Log(LogLevel.Error, $"Exception occured: {e.StackTrace}");

                throw;
            }
        }

        public bool IsRunning { get; private set; }
    }
}