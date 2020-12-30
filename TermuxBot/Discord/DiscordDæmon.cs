using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using TermuxBot.API;
using TermuxBot.Controllers;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace TermuxBot.Discord
{
    public class DiscordDæmon
    {
        public static readonly string[] TermuxPrefixes = new string[] { "termux ", "$ " };

        private ILogger<HomeController> _logger;
        private IPluginController _pluginController;

        public DiscordDæmon(ILogger<HomeController> logger, IPluginController pluginController)
        {
            _logger = logger;
            _pluginController = pluginController;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.Log(LogLevel.Information, "Starting Dicord Deamon...");

                // Try to read token from Environment variable
                string token = Environment.GetEnvironmentVariable("BOT_TOKEN");

                // If no variable set, read Token from file
                if (String.IsNullOrEmpty(token))
                {
                    if (File.Exists("./discord.token"))
                    {
                        token = await File.ReadAllTextAsync("./discord.token", cancellationToken);
                    }
                    else
                    {
                        File.Create("./discord.token");
                        throw new FileNotFoundException($"Token in File '{Path.Combine(Environment.CurrentDirectory, "discord.token")}' not found! ");
                    }
                }
                else
                {
                    await File.WriteAllTextAsync("./discord.token", token, cancellationToken);
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

            if (!e.Channel.IsPrivate && TermuxPrefixes.All(curPrefix => !e.Message.Content.ToLower().StartsWith(curPrefix))) { return; }

            string message = e.Message.Content.ToLower();
            string usedPrefix = TermuxPrefixes.FirstOrDefault(curPrefix => message.StartsWith(curPrefix));
            string responseText = String.Empty;
            DiscordMessage response = null;

            responseText = $"```{message}" + Environment.NewLine;

            if (!String.IsNullOrEmpty(usedPrefix))
            {
                message = message.Replace(usedPrefix, "");
            }

            // Message is added to response
            if (!e.Channel.IsPrivate) { await e.Message.DeleteAsync(); }

            if (message.StartsWith("ping "))
            {
                responseText += $"Pinging Termux [::1] with 32 bytes of data:```";
                response = await e.Message.RespondAsync(responseText);
                for (int i = 0; i < 4; i++)
                {
                    await Task.Delay(500);

                    responseText = responseText.Remove(responseText.Length - 3, 3) + Environment.NewLine + $"Reply from ::1: time<1ms```";
                    await response.ModifyAsync(responseText);
                }
                return;
            }

            if (message.StartsWith("help"))
            {
                responseText += "TermuX Console Bot 1.0.0" + Environment.NewLine +
                                "Send me some Console Commands, e. g.:" + Environment.NewLine +
                                "PS Get-Help" + Environment.NewLine +
                                "Bash help" + Environment.NewLine +
                                "CMD HELP```";

                response = await e.Message.RespondAsync(responseText);
                return;
            }

            // Plugin handling
            TextWriter answerTextWriter = new StringWriter();
            if (message.StartsWith("ps "))
            {
                await _pluginController.Invoke(message, answerTextWriter);

                responseText += $"{answerTextWriter}```";
                if (!String.IsNullOrEmpty(answerTextWriter.ToString())) { response = await e.Message.RespondAsync(responseText); }

                return;
            }

            if (message.StartsWith("bash "))
            {
                await e.Message.RespondAsync("```Bash Linux console have not yet been implemented.```");
                return;
            }

            if (message.StartsWith("cmd "))
            {
                await e.Message.RespondAsync("```CMD Windows console have not yet been implemented.```");
                return;
            }

            await e.Message.RespondAsync("```Unknown Command.```");
        }

        public bool IsRunning { get; private set; }
    }
}