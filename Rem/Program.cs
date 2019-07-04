using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Rem.Bot;
using Rem.Models;
using Rem.WebApi;
using Sentry;

namespace Rem
{
    class Program
    {
#if DEBUG
        private const string DefaultEnvironmentName = "Development";
#else
        private const string DefaultEnvironmentName = "Production";
#endif
        static int Main(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNET_ENVIRONMENT") ??
                                  Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                                  DefaultEnvironmentName;

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
            Console.WriteLine($"Running in {environmentName}");
            Console.WriteLine($"Bot build version: {version}");
            Console.WriteLine($"Loading appsettings from {appsettingsPath}");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(Path.Join(appsettingsPath, "appsettings.json"), optional: false, reloadOnChange: true)
                .AddJsonFile(Path.Join(appsettingsPath, $"appsettings.{environmentName}.json"), optional: true, reloadOnChange: true);
            var configuration = builder.Build();
            var connectionString = configuration.GetConnectionString("Database");

            var dbContextOptionsBuilder = new DbContextOptionsBuilder<BotContext>();
            dbContextOptionsBuilder.UseSqlite(connectionString);

            using (var dbContext = new BotContext(dbContextOptionsBuilder.Options))
            {
                dbContext.Database.Migrate();
            }

            var source = new CancellationTokenSource();

            var bot = new DiscordBot(version, configPath, connectionString);
            var botTask = bot.Start();

            var webserverTask = CreateWebHostBuilder(args, configuration).Build().StartAsync(source.Token);
            Task.WaitAll(botTask, webserverTask, Task.Delay(-1, source.Token));

            return 0;
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args, IConfiguration configuration) =>
            WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(configuration)
                .UseStartup<Startup>();
    }
}
