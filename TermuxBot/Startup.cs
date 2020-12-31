using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using TermuxBot.API;
using TermuxBot.Controllers;
using TermuxBot.Discord;

namespace TermuxBot
{
    public class Startup
    {
        private Task _deamonTask;
        private DiscordDaemon _discordDeamon;
        private ILogger _logger;
        private PluginController _pluginController;

        public Startup(IConfiguration configuration)
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder => builder
                .AddConsole()
                .AddFilter(level => level >= LogLevel.Information)
            );
            var loggerFactory = serviceCollection.BuildServiceProvider().GetService<ILoggerFactory>();

            _logger = new Logger<Startup>(loggerFactory);

            this.Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public async void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            // Init Plugins
            _pluginController = new PluginController(_logger);

            // Init Discord Deamon
            _discordDeamon = new DiscordDaemon(_logger, _pluginController);

            if (!_pluginController.Initialized)
            {
                try
                {
                    await _pluginController.InitializeAllPlugins("");
                }
                catch (Exception e)
                {
                    _logger.Log(LogLevel.Critical, "Unable to initialize plugins: " + e.Message);
                }
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
        }

        private void OnDeamon_Exited(Task obj)
        {
            _logger.Log(LogLevel.Critical, "Dicord Deamon exited");
        }

        public IConfiguration Configuration { get; }
    }
}