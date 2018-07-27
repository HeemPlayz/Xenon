#region

using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

#endregion

namespace Xenon.Services.External
{
    public class ConfigurationService
    {
        [JsonProperty("BotDiscordInviteLink")] public readonly string BotDiscordInviteLink = "";

        [JsonProperty("BotPrefixes")] public readonly string[] BotPrefixes = {""};

        [JsonProperty("BotToken")] public readonly string BotToken = "";

        [JsonProperty("DatabaseConnectionString")]
        public readonly string DatabaseConnectionString = "";

        [JsonProperty("DefaultLeaveMessage")] public readonly string DefaultLeaveMessage = "";

        [JsonProperty("DefaultLevelUpMessage")]
        public readonly string DefaultLevelUpMessage = "";

        [JsonProperty("DefaultWelcomeMessage")]
        public readonly string DefaultWelcomeMessage = "";

        [JsonProperty("GiphyApiKey")] public readonly string GiphyApiKey = "";

        [JsonProperty("KsoftApiKey")] public readonly string KsoftApiKey = "";

        [JsonProperty("LiscordApiKey")] public readonly string LiscordApiKey = "";

        [JsonProperty("LolApiKey")] public readonly string LolApiKey = "";

        [JsonProperty("OwnerIds")] public readonly ulong[] OwnerIds = { };

        public static ConfigurationService LoadNewConfig()
        {
            const string fileName = "config.json";
            if (!File.Exists(fileName))
            {
                File.CreateText(fileName).Close();
                File.WriteAllText(fileName, JsonConvert.SerializeObject(new ConfigurationService()));
                throw new Exception("No config.json file provided so I created one - set the values now");
            }

            var json = File.ReadAllText(fileName, Encoding.UTF8);
            return JsonConvert.DeserializeObject<ConfigurationService>(json);
        }
    }
}