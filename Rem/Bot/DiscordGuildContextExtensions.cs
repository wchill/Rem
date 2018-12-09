using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Rem.Bot
{
    public static class DiscordGuildContextExtensions
    {
        private static readonly string UserIdPattern = @"^<@(\d+)>$";
        private static readonly string ChannelIdPattern = @"^<#(\d+)>$";
        public static async Task<string> ResolveIds(this Discord.IGuild guild, string text)
        {
            var words = text.Split(' ');
            for (var i = 0; i < words.Length; i++)
            {
                var word = words[i];
                var channelMatch = Regex.Match(word, ChannelIdPattern);
                var userMatch = Regex.Match(word, UserIdPattern);
                if (channelMatch.Success)
                {
                    var success = ulong.TryParse(channelMatch.Groups[1].Value, out ulong id);
                    if (!success)
                    {
                        continue;
                    }
                    var channel = await guild.GetChannelAsync(id);
                    if (channel != null)
                    {
                        words[i] = $"#{channel.Name}";
                    }
                }
                else if (userMatch.Success)
                {
                    var success = ulong.TryParse(userMatch.Groups[1].Value, out ulong id);
                    if (!success)
                    {
                        continue;
                    }
                    var user = await guild.GetUserAsync(id);
                    if (user != null)
                    {
                        words[i] = user.Nickname ?? user.Username;
                    }
                }
            }

            return string.Join(' ', words);
        }
    }
}
