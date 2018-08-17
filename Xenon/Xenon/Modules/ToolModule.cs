using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using RiotSharp;
using RiotSharp.Misc;
using Voltaic;
using Xenon.Core;
using Xenon.Services;

namespace Xenon.Modules
{
    [CommandCategory(CommandCategory.Tools)]
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
    }
}