using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using RestSharp;
using RestSharp.Deserializers;

namespace Rem.Commands
{
    [Name("Urban Dictionary")]
    public class UrbanDictionaryModule : ModuleBase
    {
        internal class UdDefinition
        {
            [DeserializeAs(Name = "defid")]
            public int Id { get; set; }
            public string Word { get; set; }
            public string Author { get; set; }
            public string Permalink { get; set; }
            public string Definition { get; set; }
            public string Example { get; set; }
            public int ThumbsUp { get; set; }
            public int ThumbsDown { get; set; }
        }

        internal class UdApiResponse
        {
            public List<string> Tags { get; set; }
            public string ResultType { get; set; }
            public List<UdDefinition> List { get; set; }
            public List<string> Sounds { get; set; }
        }

        private readonly RestClient _client;
        public UrbanDictionaryModule()
        {
            _client = new RestClient("https://api.urbandictionary.com");
        }

        [Command("urban"), Summary("Looks up a term on Urban Dictionary")]
        [Alias("ud")]
        public async Task LookupDefinition([Summary("Looks up a term on Urban Dictionary")][Remainder] string text = null)
        {
            var builder = new EmbedBuilder();

            var result = await QueryApi(text);
            var bestResult = PickBestDefinition(text, result.List);

            if (bestResult == null)
            {
                await ReplyAsync($"No definitions found for \"{text}\"");
                return;
            }

            builder.WithTitle(bestResult.Word);
            builder.WithColor(0, 255, 0);
            
            builder.WithDescription(TrimToLength(bestResult.Definition, 2048));

            if (!string.IsNullOrWhiteSpace(bestResult.Example))
            {
                builder.AddField("Example", $"{TrimToLength(bestResult.Example, 1024)}");
            }

            var voteDiff = bestResult.ThumbsUp;
            var totalVotes = bestResult.ThumbsUp + bestResult.ThumbsDown;
            var percent = totalVotes > 0 ? (double) voteDiff / totalVotes * 100 : 100;
            var editDist = GetDistance(text, bestResult.Word);
            builder.WithFooter($"+{bestResult.ThumbsUp}/-{bestResult.ThumbsDown} ({percent:F2}% positive, {editDist} dist, {CalculateMetric(bestResult):F3} rating)");
            builder.WithUrl(bestResult.Permalink);
            await ReplyAsync("", embed: builder.Build());
        }

        private UdDefinition PickBestDefinition(string query, List<UdDefinition> definitions)
        {
            if (definitions.Count == 0) return null;

            var defByEditDistance = new Dictionary<int, List<UdDefinition>>();
 
            foreach (var def in definitions)
            {
                var editDistance = GetDistance(query, def.Word);
                if (!defByEditDistance.ContainsKey(editDistance))
                {
                    defByEditDistance.Add(editDistance, new List<UdDefinition>());
                }
                defByEditDistance[editDistance].Add(def);
            }

            var lowestDistance = defByEditDistance.Keys.OrderBy(k => k).First();

            return defByEditDistance[lowestDistance].OrderByDescending(CalculateMetric).First();
        }

        private int GetDistance(string s1, string s2)
        {
            return Math.Max(0, Fastenshtein.Levenshtein.Distance(s1.ToLower(), s2.ToLower()) - 1);
        }

        private double CalculateMetric(UdDefinition definition)
        {
            var totalVotes = definition.ThumbsUp + definition.ThumbsDown;
            var upvotePercentage = totalVotes > 0 ? (double)definition.ThumbsUp / totalVotes : 0.5;
            return (upvotePercentage - 0.5) * Math.Log10(totalVotes + 1);
        }

        private async Task<UdApiResponse> QueryApi(string query)
        {
            var request = new RestRequest("v0/define", Method.GET);
            request.AddParameter("term", query);
            var response = await _client.ExecuteTaskAsync<UdApiResponse>(request);
            return response.Data;
        }

        private string TrimToLength(string text, int length)
        {
            if (text.Length > length)
            {
                return text.Substring(0, length - 3) + "...";
            }

            return text;
        }
    }
}
