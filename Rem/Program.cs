using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Rem.Bot;
using Rem.WebApi;
using Sentry;

namespace Rem
{
    class Program
    {
        static int Main(string[] args)
        {
            string configPath;
            string appsettingsPath;
            string version = File.ReadAllText("VERSION.txt");

            if (args.Length >= 2)
            {
                configPath = args[0];
                appsettingsPath = args[1];
            }
            else
            {
                configPath = Environment.GetEnvironmentVariable("REM_CONFIG_FILE");
                appsettingsPath = Environment.GetEnvironmentVariable("REM_APPSETTINGS_PATH");
            }

            if (configPath == null || appsettingsPath == null)
            {
                Console.Error.WriteLine("Config file path is required (either as args or as environment variables)");
                return 1;
            }

            Console.WriteLine($"Using {configPath} for config file");

            if (!File.Exists(configPath))
            {
                Console.Error.WriteLine("Config file does not exist. One will be created. Please fill in the values.");
                BotState.Initialize(version, configPath);
                return 1;
            }
            Console.WriteLine($"Bot build version: {version}");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(appsettingsPath, optional: false, reloadOnChange: true);
            var configuration = builder.Build();
            var connectionString = configuration.GetConnectionString("Database");

            var source = new CancellationTokenSource();

            var bot = new DiscordBot(version, configPath, connectionString);
            var botTask = bot.Start();

            var webserverTask = CreateWebHostBuilder(args).Build().StartAsync(source.Token);
            Task.WaitAll(botTask, webserverTask, Task.Delay(-1, source.Token));

            return 0;
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
