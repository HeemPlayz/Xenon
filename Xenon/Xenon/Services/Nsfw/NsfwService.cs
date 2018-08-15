#region

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;
using Xenon.Core;
using Xenon.Services.External;

#endregion

namespace Xenon.Services.Nsfw
{
    public class NsfwService
    {
        private readonly HttpClient _httpClient;
        private readonly Random _random;

        public NsfwService(HttpClient httpClient, Random random)
        {
            _httpClient = httpClient;
            _random = random;
        }

        public async Task SendImageFromCategory(ShardedCommandContext context, string category, Server server)
        {
            var link =
                $"{JObject.Parse(await _httpClient.GetStringAsync($"https://nekobot.xyz/api/image?type={category}"))["message"]}";

            var embed = new EmbedBuilder()
                .WithImageUrl(link);
            embed.NormalizeEmbed(ColorType.Normal, _random, server, true, context);

            await context.Channel.SendMessageAsync(embed: embed.Build());
        }
    }
}