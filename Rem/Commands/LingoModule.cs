using Discord;
using Discord.Commands;
using Rem.Bot;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using BabelJam;

namespace Rem.Commands
{
    [Name("LingoJam")]
    public class LingoModule : ModuleBase
    {
        [Command("lingo"), Summary("Translate text into another language using LingoJam")]
        [Alias("tl")]
        public async Task TranslateText([Summary("Translator to use")] string translator, [Summary("Text to translate")][Remainder] string text)
        {
            try
            {
                string id;
                try
                {
                    var uri = new Uri(translator);
                    id = HttpUtility.UrlDecode(uri.AbsolutePath.Substring(1));
                }
                catch (UriFormatException)
                {
                    id = translator;
                }

                var parser = new LingoParser();
                var language = await parser.ParseLingoTranslation($"https://lingojam.com/{HttpUtility.UrlEncode(id)}");

                if (language.UsesCustomJavascript)
                {
                    await ReplyAsync("Unable to translate using this translator because it uses custom JavaScript.");
                    return;
                }

                var builder = new EmbedBuilder();
                builder.WithTitle($"Translation ({id} via LingoJam emulation)");
                builder.AddField("Original", text);
                builder.AddField("Translated", language.Translate(text));

                await ReplyAsync("", embed: builder.Build());
            }
            catch (Exception)
            {
                await ReplyAsync("An error occurred while performing translation.");
                throw;
            }
        }
    }
}
