﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using SQLite;
using Image = Rem.Models.Image;

namespace Rem.Bot
{
    public class DiscordBot
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly ServiceCollection _services;
        private readonly BotState _state;
        private readonly SQLiteAsyncConnection _dbContext;

        public DiscordBot(string version, string configPath, string dbPath)
        {
            _state = BotState.Initialize(version, configPath);
            _dbContext = new SQLiteAsyncConnection(dbPath);

            _client = new DiscordSocketClient();
            _client.Log += Log;
            _client.Ready += () => Log($"Running as user {_client.CurrentUser.Username} ({_client.CurrentUser.Id})");

            _commands = new CommandService();
            _services = new ServiceCollection();
        }

        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private static Task Log(string msg)
        {
            Console.WriteLine(msg);
            return Task.CompletedTask;
        }

        private async Task InstallCommands()
        {
            await _dbContext.CreateTableAsync<Image>();

            _services.AddSingleton(_dbContext);
            _services.AddSingleton(_state);

            _client.MessageReceived += MessageReceived;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task MessageReceived(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            // Create a number to track where the prefix ends and the command begins
            var argPos = 0;

            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if (!(message.HasStringPrefix(_state.Prefix, ref argPos) ||
                  message.HasMentionPrefix(_client.CurrentUser, ref argPos))) return;

            // Create a Command Context
            var context = new CommandContext(_client, message);

            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed successfully)
            var result = await _commands.ExecuteAsync(context, argPos, _services.BuildServiceProvider());

            /*
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ErrorReason);
            */

            if (!result.IsSuccess)
            {
                switch (result.Error)
                {
                    case CommandError.Exception:
                        if (result is ExecuteResult execResult)
                        {
                            //Debugger.Break();
                            await Console.Error.WriteLineAsync($"Error encountered when handling command {message}:");
                            await Console.Error.WriteLineAsync(execResult.Exception.ToString());
                        }

                        break;
                    //TODO: Handle other CommandError cases
                }
            }

            await UpdateDiscordStatus();
            await _state.PersistState();
        }

        private async Task UpdateDiscordStatus()
        {
            await _client.SetGameAsync($"{_state.Version}");
            // await _client.SetGameAsync($"{_state.PatCount} pats given, {_state.BribeCount} mods bribed");
        }

        public async Task Start()
        {
            await InstallCommands();
            await _client.LoginAsync(TokenType.Bot, _state.ClientSecret);
            await _client.StartAsync();
            await UpdateDiscordStatus();
        }
    }
}
