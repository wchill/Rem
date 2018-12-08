using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

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

        [JsonIgnore] 
        public string FilePath { get; private set; }

        public static BotState Initialize(string version, string filePath)
        {
            try
            {
                var state = JsonConvert.DeserializeObject<BotState>(File.ReadAllText(filePath));
                state.FilePath = filePath;
                state.AdminList = state.AdminList ?? new HashSet<ulong>();

                Task.WaitAll(state.PersistState());
                state.Version = version;
                return state;
            }
            //TODO: Handling of exception needed.
            catch (Exception e)
            {
                Console.Error.WriteLine("Failed to read text from {0} with error: {1}", filePath, e);
                var state = new BotState
                {
                    FilePath = filePath,
                    AdminList = new HashSet<ulong>()
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
