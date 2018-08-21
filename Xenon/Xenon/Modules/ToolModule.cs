#region

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using RiotSharp;
using Xenon.Core;

#endregion

namespace Xenon.Modules
{
    [CommandCategory(CommandCategory.Tools)]
    [CheckState]
    public class ToolModule : CommandBase
    {
        private readonly Configuration _configuration;
        private readonly RiotApi _riotApi;

        public ToolModule(Configuration configuration)
        {
            _configuration = configuration;
            _riotApi = RiotApi.GetInstance(_configuration.LolApiKey, 1500, 90000);
        }

        [Command("encode")]
        [Alias("ec")]
        [Summary("Encodes some text")]
        public async Task EncodeAsync([Remainder] string text)
        {
            await Context.Message.DeleteAsync();
            var bytes = Encoding.UTF8.GetBytes(text).Select(x => (byte) (x + (byte) Context.User.Id % 3)).ToArray();
            var encoded = Convert.ToBase64String(bytes);
            await ReplyEmbedAsync("Encoded Text", $"{encoded}");
        }

        [Command("decode")]
        [Alias("dc")]
        [Summary("Decodes some text")]
        public async Task DecodeAsync([Remainder] string text)
        {
            var bytes = Convert.FromBase64String(text).Select(x => (byte) (x - (byte) Context.User.Id % 3)).ToArray();
            var decoded = Encoding.UTF8.GetString(bytes);
            await ReplyEmbedAsync("Decoded Text", $"{decoded}");
        }

        [CommandCategory(CommandCategory.Tools)]
        [Group("robot")]
        [Summary("Shows someone as robot")]
        public class Robot : CommandBase
        {
            private readonly Random _random;

            public Robot(Random random)
            {
                _random = random;
            }

            [Command("")]
            public async Task RobotAsync()
            {
                await RobotAsync(Context.User);
            }

            [Command("")]
            [CheckServer]
            public async Task RobotAsync(SocketUser user)
            {
                await ReplyEmbedAsync(new EmbedBuilder()
                    .WithTitle($"{(user as IGuildUser)?.Nickname ?? user.Username} as robot").WithImageUrl(
                        $"https://robohash.org/{HttpUtility.UrlEncode((user as IGuildUser)?.Nickname ?? user.Username)}?set={_random.Next(1, 4)}"));
            }
        }
    }
}