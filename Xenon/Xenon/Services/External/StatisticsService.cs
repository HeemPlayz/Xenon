using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Listcord.Net;
using Xenon.Core;

namespace Xenon.Services.External
{
    public class StatisticsService
    {
        private readonly DiscordShardedClient _client;
        private readonly Configuration _configuration;
        private readonly ListcordClient _listcord;

        public StatisticsService(Configuration configuration, DiscordShardedClient client)
        {
            _configuration = configuration;
            _client = client;
            _listcord = new ListcordClient(_configuration.LiscordApiKey);

            _client.ShardReady += ShardReady;
            _client.JoinedGuild += GuildUpdated;
            _client.LeftGuild += GuildUpdated;
        }

        private async Task GuildUpdated(SocketGuild arg)
        {
            await UpdateStatistics();
        }

        private async Task ShardReady(DiscordSocketClient client)
        {
            await UpdateStatistics();
        }

        private async Task UpdateStatistics()
        {
            foreach (var shard in _client.Shards)
                await shard.SetActivityAsync(new Game(
                    $"for commands | {_configuration.BotPrefixes.First()}help | shard {shard.ShardId + 1}/{_client.Shards.Count} | {_client.Guilds.Count} servers",
                    ActivityType.Watching));
            await _listcord.PostBotGuildsAsync(_client.CurrentUser.Id, (ulong) _client.Guilds.Count);
        }
    }
}