#region

using System;
using Newtonsoft.Json;

#endregion

namespace Xenon.Core
{
    public class Configuration
    {
        [JsonProperty("BotDiscordInviteLink")] public readonly string BotDiscordInviteLink = "";

        [JsonProperty("BotPrefixes")] public readonly string[] BotPrefixes = {""};

        [JsonProperty("BotToken")] public readonly string BotToken = "";

        [JsonProperty("DatabaseConnectionString")]
        public readonly string DatabaseConnectionString = "";

        [JsonProperty("DefaultJoinMessage")] public readonly string DefaultJoinMessage = "";

        [JsonProperty("DefaultLeaveMessage")] public readonly string DefaultLeaveMessage = "";

        [JsonProperty("DefaultLevelUpMessage")]
        public readonly string DefaultLevelUpMessage = "";

        [JsonProperty("GiphyApiKey")] public readonly string GiphyApiKey = "";

        [JsonProperty("KsoftApiKey")] public readonly string KsoftApiKey = "";

        [JsonProperty("LiscordApiKey")] public readonly string LiscordApiKey = "";

        [JsonProperty("LolApiKey")] public readonly string LolApiKey = "";

        [JsonProperty("OwnerIds")] public readonly ulong[] OwnerIds = Array.Empty<ulong>();
    }
}