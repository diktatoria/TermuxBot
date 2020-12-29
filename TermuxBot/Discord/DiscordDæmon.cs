using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
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

                discord.MessageCreated += OnDiscord_MessageCreated;

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

        private async Task OnDiscord_MessageCreated(MessageCreateEventArgs e)
        {
            if (e.Author.Username == "Termux") { return; }

            if (!e.Channel.IsPrivate && !e.Message.Content.ToLower().StartsWith("termux ")) { return; }

            string message = e.Message.Content.ToLower();
            if (message.StartsWith("termux "))
            {
                message = message.Replace("termux ", "");
            }

            if (message.StartsWith("ping"))
            {
                await e.Message.RespondAsync("pong!");
                return;
            }

            if (message.StartsWith("help"))
            {
                await e.Message.RespondAsync("TermuX Console Bot");
                await e.Message.RespondAsync("Send me some Console Commands, e. g.:");
                await e.Message.RespondAsync("PS Get-Help" + Environment.NewLine +
                                                    "Bash help" + Environment.NewLine +
                                                    "CMD HELP" + Environment.NewLine);
                return;
            }

            if (message.StartsWith("ps "))
            {
                await e.Message.RespondAsync("Powershell plugin has not been loaded.");
                return;
            }

            if (message.StartsWith("bash "))
            {
                await e.Message.RespondAsync("Bash Linux console have not yet been implemented.");
                return;
            }

            if (message.StartsWith("cmd "))
            {
                await e.Message.RespondAsync("CMD Windows console have not yet been implemented.");
                return;
            }
        }

        public bool IsRunning { get; private set; }
    }
}