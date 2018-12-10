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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace Rem.Services
{
    [Service(typeof(IMemeGeneratorService))]
    public class MemeGeneratorService : IMemeGeneratorService
    {
        private readonly CommandService _commandService;
        private readonly IParameterExpansionService _expansionService;
        private readonly Dictionary<string, MemeTemplate> _commandAliasToTemplates;
        private readonly Dictionary<string, MemeTemplate> _commandNameToTemplates;

        private ModuleInfo _module;

        public MemeGeneratorService(CommandService commandService, IParameterExpansionService expansionService, Dictionary<string, MemeTemplate> templates = null)
        {
            _commandService = commandService;
            _expansionService = expansionService;
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
                    .WithName("Auto Meme Generator");

                foreach (var kvp in _commandAliasToTemplates)
                {
                    CreateCommand(kvp.Key, kvp.Value, module);
                    CreateDebugCommand($"{kvp.Key}-debug", kvp.Value, module);
                }
            });
        }

        private void CreateCommand(string trigger, MemeTemplate template, ModuleBuilder module)
        {
            module.AddCommand(trigger, CommandCallbackAsync, command =>
            {
                command
                    .WithName(template.Name)
                    .WithSummary(template.Description)
                    .WithRunMode(RunMode.Async);
                AddParameters(template, command);
            });
        }

        private void CreateDebugCommand(string trigger, MemeTemplate template, ModuleBuilder module)
        {
            module.AddCommand(trigger, DebugCommandCallbackAsync, command =>
            {
                command
                    .WithName(template.Name)
                    .WithSummary(template.Description)
                    .WithRunMode(RunMode.Async)
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
                var meme = template.CreateMeme(allLayers);
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
                using (var meme = template.CreateMeme(expandedParameters))
                {
                    await SendImage(meme, context.Channel, template.Name);
                }
            }
        }

        private async Task SendImage(Image<Rgba32> image, IMessageChannel channel, string imagename)
        {
            using (var memoryStream = new MemoryStream())
            {
                image.Save(memoryStream, new PngEncoder());
                memoryStream.Seek(0, SeekOrigin.Begin);
                await channel.SendFileAsync(memoryStream, $"{imagename}-.png");
            }
        }
    }
}