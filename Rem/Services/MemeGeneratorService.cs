using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MemeGenerator;
using Rem.Attributes;
using SixLabors.ImageSharp.Formats.Png;

namespace Rem.Services
{
    [Service(typeof(IMemeGeneratorService))]
    public class MemeGeneratorService : IMemeGeneratorService
    {
        private readonly CommandService _commandService;
        private readonly IParameterExpansionService _expansionService;
        private readonly Dictionary<string, MemeTemplate> _templates;

        private ModuleInfo _module;

        public MemeGeneratorService(CommandService commandService, IParameterExpansionService expansionService, Dictionary<string, MemeTemplate> templates = null)
        {
            _commandService = commandService;
            _expansionService = expansionService;
            _templates = templates ?? new Dictionary<string, MemeTemplate>();
        }

        public MemeGeneratorService AddTemplate(string name, MemeTemplate template)
        {
            _templates[name] = template;
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

                foreach (var kvp in _templates)
                {
                    module.AddCommand(kvp.Key, CommandCallbackAsync, command =>
                    {
                        var template = kvp.Value;
                        command
                            .WithName(template.Name)
                            .WithSummary(template.Description)
                            .WithRunMode(RunMode.Async);

                        // TODO: Handle variadic arguments
                        // TODO: Handle optional arguments
                        // TODO: Handle default arguments
                        if (template.InputFields.Count == 1)
                        {
                            var field = template.InputFields.First();
                            command.AddParameter<string>(field.Name, builder =>
                            {
                                builder
                                    .AddAttributes(new RemainderAttribute())
                                    .WithSummary(field.Description);
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
                    });
                }
            });
        }

        private async Task CommandCallbackAsync(ICommandContext context, object[] parameters, IServiceProvider services, CommandInfo info)
        {
            var exists = _templates.TryGetValue(info.Name, out var template);
            if (!exists)
            {
                return;
            }

            var expandedParameters = parameters.Select(param => _expansionService.Expand(param as string));

            using (var meme = template.CreateMeme(expandedParameters))
            {
                var memoryStream = new MemoryStream();
                meme.Save(memoryStream, new PngEncoder());
                await context.Channel.SendFileAsync(memoryStream, template.Name);
            }
        }
    }
}