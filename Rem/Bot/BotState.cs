using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SQLite;

namespace Rem.Bot
{
    public class BotState
    {
        // These values cannot change without restarting bot
        public string ClientSecret { get; set; }
        [JsonIgnore]
        public string Version { get; private set; }

        // These values can change
        public string Prefix { get; set; }
        public int PatCount { get; set; }
        public int BribeCount { get; set; }
        public HashSet<ulong> AdminList { get; set; }
        public string TranslatorApiKey { get; set; }
        public HashSet<string> BannedRoles { get; set; }

        [JsonIgnore]
        public string FilePath { get; private set; }

        [JsonIgnore]
        public DateTime ConnectionTime { get; set; }

        public static BotState Initialize(string version, string filePath)
        {
            try
            {
                var state = JsonConvert.DeserializeObject<BotState>(File.ReadAllText(filePath));
                state.FilePath = filePath;
                if (state.AdminList == null)
                {
                    state.AdminList = new HashSet<ulong>();
                }
                if (state.BannedRoles == null)
                {
                    state.BannedRoles = new HashSet<string>();
                }
                Task.WaitAll(state.PersistState());
                state.Version = version;
                return state;
            }
            catch (Exception)
            {
                var state = new BotState
                {
                    FilePath = filePath,
                    AdminList = new HashSet<ulong>(),
                    BannedRoles = new HashSet<string>()
                };
                Task.WaitAll(state.PersistState());
                state.Version = version;
                return state;
            }
        }

        public async Task ReloadState()
        {
            var text = await File.ReadAllTextAsync(FilePath);
            var newState = JsonConvert.DeserializeObject<BotState>(text);

            // TODO: Find a way to do this dynamically
            Prefix = newState.Prefix;
            PatCount = newState.PatCount;
            BribeCount = newState.BribeCount;
            TranslatorApiKey = newState.TranslatorApiKey;
        }

        public async Task PersistState()
        {
            await File.WriteAllTextAsync(FilePath, JsonConvert.SerializeObject(this));
        }
    }
}
