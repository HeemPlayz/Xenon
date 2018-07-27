#region

using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Listcord.Net;

#endregion

namespace Xenon.Services.External
{
    public class StatisticsUpdateServer
    {
        private readonly DiscordClient _client;
        private readonly ConfigurationService _configurationService;
        private readonly ListcordClient _listcordClient;

        public StatisticsUpdateServer(ConfigurationService configurationService, DiscordClient client)
        {
            _configurationService = configurationService;
            _client = client;
            _listcordClient = new ListcordClient(_configurationService.LiscordApiKey);
        }

        public async Task GuildUpdate(GuildDeleteEventArgs guildDeleteEventArgs)
        {
            await UpdateGuilds();
        }

        public async Task GuildUpdate(GuildCreateEventArgs e)
        {
            await UpdateGuilds();
        }

        public async Task Ready(ReadyEventArgs e)
        {
            await UpdateGuilds();
        }

        private async Task UpdateGuilds()
        {
            await _client.UpdateStatusAsync(new DiscordActivity(
                $"for commands | >help | {_client.Guilds.Count} servers",
                ActivityType.Watching));
            await _listcordClient.PostBotGuildsAsync(_client.CurrentUser.Id, (ulong) _client.Guilds.Count);
        }
    }
}