#region

using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;

#endregion

namespace Xenon.Services.Nsfw
{
    public class NsfwService
    {
        private readonly HttpClient _httpClient;

        public NsfwService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task SendImageFromCategory(CommandContext ctx, string category)
        {
            var link =
                $"{JObject.Parse(await _httpClient.GetStringAsync($"https://nekobot.xyz/api/image?type={category}"))["message"]}";

            var embed = new DiscordEmbedBuilder()
                .WithImageUrl(link)
                .WithColor(DiscordColor.Purple)
                .WithFooter($"Requested by {ctx.Member?.DisplayName ?? ctx.User.Username}", ctx.User.AvatarUrl);

            await ctx.RespondAsync(embed: embed);
        }
    }
}