using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rem.Attributes;
using Rem.Extensions;
using Rem.Models;
using Rem.Services;
using Rem.Utilities;
using Sentry;
using Sentry.Protocol;

namespace Rem.Bot
{
    public class DiscordBot
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly BotState _state;
        private IServiceProvider _services;

        private readonly string _dbConnectionString;

        public DiscordBot(string version, string configPath, string dbConnectionString)
        {
            _dbConnectionString = dbConnectionString;
            _state = BotState.Initialize(version, configPath);
            SentrySdk.Init(new SentryOptions
            {
                Dsn = new Dsn(_state.SentryDsn),
                Debug = true,
                AttachStacktrace = true
            });
            
            _client = new DiscordSocketClient();
            _client.Log += Log;
            _client.Ready += OnClientReady;

            _commands = new CommandService();
            _commands.Log += Log;
            _services = null;
        }

        private async Task OnClientReady()
        {
            _state.ConnectionTime = DateTime.UtcNow;
            await Log($"Running as user {_client.CurrentUser.Username} ({_client.CurrentUser.Id})");

            var guilds = await _client.Rest.GetGuildsAsync();
            foreach (var guild in guilds)
            {
                //_dbContext.
            }
        }

        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            if (msg.Exception != null)
            {
                Console.Error.WriteLine(msg.Exception);
            }
            return Task.CompletedTask;
        }

        private static Task Log(string msg)
        {
            Console.WriteLine(msg);
            return Task.CompletedTask;
        }

        private async Task InstallServices()
        {
            var types = Assembly.GetExecutingAssembly().FindTypesWithAttribute<ServiceAttribute>().ToImmutableArray();

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton(_state)
                .AddSingleton(MemeLoader.LoadAllTemplates())
                .AddServices(types)
                .AddDbContextPool<BotContext>(options => { options.UseSqlite(_dbConnectionString); })
                .BuildServiceProvider();
        }

        private async Task InstallCommands()
        {
            _client.MessageReceived += MessageReceived;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task MessageReceived(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            // Create a number to track where the prefix ends and the command begins
            var argPos = 0;
            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if (!(message.HasStringPrefix(_state.Prefix, ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))) return;
            // Create a Command Context
            var context = new CommandContext(_client, message);

            // TODO: This only handles messages from servers, not DMs. Need to implement support for DMs.
            var user = context.User as SocketGuildUser;
            if (user != null)
            {
                // Other bots shouldn't be able to trigger commands
                if (user.IsBot)
                {
                    return;
                }

                foreach (var role in user.Roles)
                {
                    if (_state.BannedRoles.Contains(role.Name.ToLower()))
                    {
                        await context.Channel.SendMessageAsync($"Role \"{role.Name}\" is banned from using bot commands.");
                        return;
                    }
                }
            }
            else
            {
                return;
            }
            
            SentrySdk.WithScope(async scope =>
            {
                scope.User = new User
                {
                    Id = user.Id.ToString(),
                    Username = $"{user.Username}#{user.DiscriminatorValue}"
                };
                scope.SetExtra("command", message.Content);
                scope.SetExtra("message_channel", $"#{message.Channel.Name} ({message.Channel.Id})");

                // Execute the command. (result does not indicate a return value, 
                // rather an object stating if the command executed successfully)
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (result.IsSuccess) return;

                switch (result.Error)
                {
                    case CommandError.Exception:
                        if (result is ExecuteResult execResult)
                        {
                            await Console.Error.WriteLineAsync($"Error encountered when handling command {message}:");
                            await Console.Error.WriteLineAsync(execResult.Exception.ToString());
                            SentrySdk.CaptureException(execResult.Exception);
                        }
                        break;
                    case CommandError.UnknownCommand:
                        break;
                    case CommandError.ParseFailed:
                        await context.Channel.SendMessageAsync(result.ErrorReason);
                        await Console.Error.WriteLineAsync($"Error: {result.ErrorReason}");
                        break;
                    case CommandError.BadArgCount:
                        await context.Channel.SendMessageAsync(result.ErrorReason);
                        await Console.Error.WriteLineAsync($"Error: {result.ErrorReason}");
                        break;
                    case CommandError.ObjectNotFound:
                        await context.Channel.SendMessageAsync(result.ErrorReason);
                        await Console.Error.WriteLineAsync($"Error: {result.ErrorReason}");
                        break;
                    case CommandError.MultipleMatches:
                        await context.Channel.SendMessageAsync(result.ErrorReason);
                        await Console.Error.WriteLineAsync($"Error: {result.ErrorReason}");
                        break;
                    case CommandError.UnmetPrecondition:
                        await context.Channel.SendMessageAsync($"<@{message.Author.Id}>: {result.ErrorReason}");
                        break;
                    case CommandError.Unsuccessful:
                        await context.Channel.SendMessageAsync(result.ErrorReason);
                        await Console.Error.WriteLineAsync($"Error: {result.ErrorReason}");
                        break;
                    default:
                        await Console.Error.WriteLineAsync($"Unknown result type: {result.Error}");
                        await Console.Error.WriteLineAsync($"Error: {result.ErrorReason}");
                        break;
                }
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            });

            await _state.PersistState();
        }

        public async Task Start()
        {
            await InstallServices();
            await InstallCommands();
            await _client.LoginAsync(TokenType.Bot, _state.ClientSecret);
            await _client.StartAsync();
            _services.RunInitMethods();

            await _client.SetGameAsync($"{_state.Version}");
        }
    }
}
