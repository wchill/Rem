using MemeGenerator;
using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using System.Linq;

namespace Rem.Utilities.MemeModels
{
    class MemeTemplates
    {
        public Dictionary<string, MemeTemplate> Templates { get; set; }

        //globally-shared renderers
        public ImageRenderer[] ImageRenderers { get; set; }
        public TextRenderer[] TextRenderers { get; set; }

        public Dictionary<string, MemeGenerator.MemeTemplate> Convert()
            => Templates.ToDictionary(k => k.Key, v => v.Value.Convert());
    }

    class MemeTemplate
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public InputField[] Inputs { get; set; }
        
        //meme-specific renderers
        public ImageRenderer[] ImageRenderers { get; set; }
        public TextRenderer[] TextRenderers { get; set; }

        internal MemeGenerator.MemeTemplate Convert()
            => new MemeGenerator.MemeTemplate(Name, Description,
                Image.Load(ImagePath),
                Inputs.Select(i => i.Convert()).ToArray());
    }
}
