using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Rem.Bot;
using RestSharp;

namespace Rem.Commands
{
    public class TranslateModule : ModuleBase
    {
        internal class SupportedLanguage
        {
            public string Name { get; set; }
            public string NativeName { get; set; }
            public string Direction { get; set; }
        }

        internal class Translation
        {
            public string Text { get; set; }
            public string To { get; set; }
        }

        internal class DetectedLanguageResponse
        {
            public string Language { get; set; }
            public double Score { get; set; }
        }

        internal class TranslationResponse
        {
            public DetectedLanguageResponse DetectedLanguage { get; set; }
            public List<Translation> Translations { get; set; }
        }

        private readonly BotState _botState;
        private readonly RestClient _client;

        public TranslateModule(BotState state)
        {
            _botState = state;
            _client = new RestClient("https://api.cognitive.microsofttranslator.com");
        }

        [Command("translate"), Summary("Translate text into another language")]
        [Alias("tl")]
        public async Task TranslateText([Summary("Text to translate")][Remainder] string text)
        {
            try
            {
                var translationResp = await GetTranslation(text);
                var translation = translationResp.Translations.First();
                var detectedLang = translationResp.DetectedLanguage;

                var builder = new EmbedBuilder();
                builder.WithTitle($"Translation ({detectedLang.Language} => {translation.To})");
                builder.AddField("Original", text);
                builder.AddField("Translated", translation.Text);
                builder.WithFooter($"Language detection confidence: {detectedLang.Score}");

                await ReplyAsync("", embed: builder.Build());
            }
            catch (ArgumentException)
            {
                await ReplyAsync("Translate API has not been set up.");
            }
        }

        [Command("translatelanguages"), Summary("List all supported translation langauges")]
        public async Task ListSupportedLanguages()
        {
            var languages = await GetSupportedLanguages();
            var description = string.Join('\n', languages.Select(kvp => $"{kvp.Value.Name} - *{kvp.Key}*"));
            var builder = new EmbedBuilder();
            builder.WithTitle($"Supported languages ({languages.Count})");
            builder.WithDescription(description);

            await ReplyAsync("", embed: builder.Build());
        }

        private async Task<Dictionary<string, SupportedLanguage>> GetSupportedLanguages()
        {
            var request = new RestRequest("languages", Method.GET);
            request.AddParameter("api-version", "3.0");
            request.AddParameter("scope", "translation");
            var response =
                await _client.ExecuteTaskAsync<Dictionary<string, Dictionary<string, SupportedLanguage>>>(request);
            return response.Data["translation"];
        }

        private async Task<TranslationResponse> GetTranslation(string text)
        {
            if (string.IsNullOrWhiteSpace(_botState.TranslatorApiKey))
            {
                throw new ArgumentException("Invalid API key");
            }

            var request = new RestRequest("translate", Method.POST);
            request.AddParameter("api-version", "3.0", ParameterType.QueryString);
            request.AddParameter("to", "en", ParameterType.QueryString);
            request.AddHeader("Ocp-Apim-Subscription-Key", _botState.TranslatorApiKey);
            request.AddJsonBody(new object[] {new {Text = text}});
            var response = await _client.ExecuteTaskAsync<List<TranslationResponse>>(request);
            return response.Data.First();
        }
    }
}
