using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Commands.Builders;
using MemeGenerator;
using Rem.Attributes;
using Rem.Bot;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace Rem.Services
{
    [Service(typeof(IMemeGeneratorService))]
    public class MemeGeneratorService : IMemeGeneratorService
    {
        private static readonly string ModuleName = "Auto Meme Generator";

        private readonly CommandService _commandService;
        private readonly IParameterExpansionService _expansionService;
        private readonly BotState _botState;

        // Maps command triggers to the meme templates.
        private readonly Dictionary<string, MemeTemplate> _commandAliasToTemplates;

        // Maps command names (not triggers) to the meme templates.
        private readonly Dictionary<string, MemeTemplate> _commandNameToTemplates;

        private ModuleInfo _module;

        public MemeGeneratorService(CommandService commandService, IParameterExpansionService expansionService, BotState botState, Dictionary<string, MemeTemplate> templates = null)
        {
            _commandService = commandService;
            _expansionService = expansionService;
            _botState = botState;
            _commandAliasToTemplates = templates ?? new Dictionary<string, MemeTemplate>();
            _commandNameToTemplates = _commandAliasToTemplates.Values.ToDictionary(template => template.Name);
        }

        public MemeGeneratorService AddTemplate(string commandAlias, MemeTemplate template)
        {
            _commandAliasToTemplates[commandAlias] = template;
            _commandNameToTemplates[template.Name] = template;
            return this;
        }

        [Init]
        public async Task CreateModule()
        {
            if (_module != null)
                await _commandService.RemoveModuleAsync(_module);
            
            _module = await _commandService.CreateModuleAsync("", module =>
            {
                module.WithSummary("Automatically generates memes")
                    .WithName(ModuleName)
                    .AddPrecondition(new RequireRoleAttribute("regulars"));

                // Add one command for each meme
                foreach (var kvp in _commandAliasToTemplates)
                {
                    CreateMemeCommand(kvp.Key, kvp.Value, module);
                    CreateMemeDebugCommand($"{kvp.Key}-debug", kvp.Value, module);
                }

                // Add help commands
                module.AddCommand("meme", MemeHelpCallbackAsync, command =>
                {
                    command
                        .WithName("Meme List")
                        .WithSummary("Get the list of memes available.");
                });
                module.AddCommand("meme", MemeHelpCallbackAsync, command =>
                {
                    command
                        .WithName("Meme Help")
                        .WithSummary("Get help for a meme.");
                    command.AddParameter<string>("command", builder =>
                    {
                        builder
                            .WithSummary("meme command")
                            .WithIsRemainder(true);
                    });
                });
            });
        }

        private void CreateMemeCommand(string trigger, MemeTemplate template, ModuleBuilder module)
        {
            module.AddCommand(trigger, CommandCallbackAsync, command =>
            {
                command
                    .WithName(template.Name)
                    .WithSummary(template.Description);
                AddParameters(template, command);
            });
        }

        private void CreateMemeDebugCommand(string trigger, MemeTemplate template, ModuleBuilder module)
        {
            module.AddCommand(trigger, DebugCommandCallbackAsync, command =>
            {
                command
                    .WithName($"{template.Name} (Debug)")
                    .WithSummary(template.Description)
                    .AddPrecondition(new RequireOwnerAttribute());
                AddParameters(template, command);
            });
        }

        private static void AddParameters(MemeTemplate template, CommandBuilder command)
        {
            // TODO: Handle variadic arguments
            // TODO: Handle optional arguments
            // TODO: Handle default arguments
            if (template.InputFields.Count == 1)
            {
                var field = template.InputFields.First();
                command.AddParameter<string>(field.Name, builder =>
                {
                    builder
                        .WithSummary(field.Description)
                        .WithIsRemainder(true);
                });
            }
            else
            {
                foreach (var field in template.InputFields)
                {
                    command.AddParameter<string>(field.Name,
                        builder => { builder.WithSummary(field.Description); });
                }
            }
        }

        private async Task DebugCommandCallbackAsync(ICommandContext context, object[] parameters,
            IServiceProvider services, CommandInfo info)
        {
            var exists = _commandNameToTemplates.TryGetValue(info.Name, out var template);
            if (!exists)
            {
                return;
            }

            // TODO: Handle non channel situations
            using (context.Channel.EnterTypingState())
            {
                var expandedParameters = parameters.Select(param => _expansionService.Expand(param as string)).ToList();
                var allLayers = template.CreateLayers(expandedParameters);
                var meme = await Task.Run(() => template.CreateMeme(allLayers));
                try
                {
                    var mask = allLayers.First();
                    await SendImage(mask, context.Channel, $"{template.Name}-mask");

                    var imageLayers = allLayers.Skip(1).ToList();
                    for (var i = 0; i < imageLayers.Count; i++)
                    {
                        var layer = imageLayers[i];
                        await SendImage(layer, context.Channel, $"{template.Name}-layer{i}");
                    }
                    
                    await SendImage(meme, context.Channel, template.Name);
                }
                finally
                {
                    meme.Dispose();
                    allLayers.Dispose();
                }
            }
        }

        private async Task CommandCallbackAsync(ICommandContext context, object[] parameters, IServiceProvider services, CommandInfo info)
        {
            var exists = _commandNameToTemplates.TryGetValue(info.Name, out var template);
            if (!exists)
            {
                return;
            }

            // TODO: Handle non channel situations
            using (context.Channel.EnterTypingState())
            {
                var expandedParameters = parameters.Select(param => _expansionService.Expand(param as string)).ToList();
                using (var meme = await Task.Run(() => template.CreateMeme(expandedParameters)))
                {
                    await SendImage(meme, context.Channel, template.Name);
                }
            }
        }

        private async Task MemeHelpCallbackAsync(ICommandContext context, object[] parameters, IServiceProvider services, CommandInfo info)
        {
            if (parameters.Length == 0)
            {
                var builder = new EmbedBuilder();

                builder.WithTitle("List of meme commands");
                builder.WithFooter($"For help with an individual meme, use {_botState.Prefix}{info.Aliases.First()} <command>");
                builder.WithColor(0, 255, 0);

                var commandNames = _commandAliasToTemplates.Keys.ToArray();
                var half = (commandNames.Length + 1) / 2;

                if (commandNames.Length == 0)
                {
                    await context.Channel.SendMessageAsync("There are no meme commands installed.");
                    return;
                }

                builder.AddField("Meme list", string.Join("\n", commandNames, 0, half), true);
                builder.AddField("Meme list", string.Join("\n", commandNames, half, commandNames.Length - half), true);

                await context.Channel.SendMessageAsync("", embed: builder.Build());
            }
            else
            {
                var alias = parameters[0] as string;
                var command = _commandService.Commands.Where(ci => ci.Module.Name == ModuleName && ci.Aliases.Contains(alias)).FirstOrDefault();
                if (command == null)
                {
                    await context.Channel.SendMessageAsync("That meme doesn't exist.");
                    return;
                }

                var builder = new EmbedBuilder();

                builder.WithTitle(command.Name);
                builder.WithColor(0, 255, 0);
                builder.WithDescription(command.Summary);
                builder.AddField("Usage", $"{_botState.Prefix}{alias} <{string.Join("> <", command.Parameters.Select(p => p.Name).ToArray())}>");

                await context.Channel.SendMessageAsync("", embed: builder.Build());
            }
        }

        private async Task SendImage(Image<Rgba32> image, IMessageChannel channel, string imagename)
        {
            using (var memoryStream = new MemoryStream())
            {
                image.Save(memoryStream, new PngEncoder());
                memoryStream.Seek(0, SeekOrigin.Begin);
                await channel.SendFileAsync(memoryStream, $"{imagename}.png");
            }
        }
    }
}