#region

using System;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;
using Xenon.Core;
using Xenon.Services;
using Xenon.Services.Nsfw;

#endregion

namespace Xenon.Modules
{
    [RequireNsfw]
    [CommandCategory(CommandCategory.Nsfw)]
    public class NsfwModule : CommandModule
    {
        private readonly HttpClient _httpClient;
        private readonly NsfwService _nsfwService;
        private readonly Random _random;
        private readonly RedditService _redditService;

        public NsfwModule(HttpClient httpClient, Random random, RedditService redditService, NsfwService nsfwService)
        {
            _httpClient = httpClient;
            _random = random;
            _redditService = redditService;
            _nsfwService = nsfwService;
        }

        [Command("ass")]
        public async Task AssAsync(CommandContext ctx)
        {
            var random = _random.Next(6012);
            var data = JArray.Parse(await _httpClient.GetStringAsync($"http://api.obutts.ru/butts/{random}")).First;
            var embed = new DiscordEmbedBuilder()
                .WithImageUrl($"http://media.obutts.ru/{data["preview"]}")
                .WithFooter($"Requested by {ctx.Member?.DisplayName ?? ctx.User.Username}", ctx.User.AvatarUrl)
                .WithColor(DiscordColor.Purple);

            await ctx.RespondAsync(embed: embed);
        }

        [Command("boobs")]
        public async Task BoobsAsync(CommandContext ctx)
        {
            var random = _random.Next(12965);
            var data = JArray.Parse(await _httpClient.GetStringAsync($"http://api.oboobs.ru/boobs/{random}")).First;
            var embed = new DiscordEmbedBuilder()
                .WithImageUrl($"http://media.oboobs.ru/{data["preview"]}")
                .WithFooter($"Requested by {ctx.Member?.DisplayName ?? ctx.User.Username}", ctx.User.AvatarUrl)
                .WithColor(DiscordColor.Purple);

            await ctx.RespondAsync(embed: embed);
        }

        [Command("hentai")]
        public async Task HentaiAsync(CommandContext ctx)
        {
            var url =
                $"{JObject.Parse(await _httpClient.GetStringAsync("https://nekos.life/api/v2/img/Random_hentai_gif"))["url"]}";
            var embed = new DiscordEmbedBuilder()
                .WithImageUrl(url)
                .WithColor(DiscordColor.Purple);
            await ctx.RespondAsync(embed: embed);
        }

        [Command("nude")]
        public async Task NudeAsync(CommandContext ctx)
        {
            await _nsfwService.SendImageFromCategory(ctx, "4k");
            //await _redditService.SendImageFromSubredditAsync(ctx, false);
        }

        [Command("nudegif")]
        public async Task NudeGifAsync(CommandContext ctx)
        {
            await _nsfwService.SendImageFromCategory(ctx, "pgif");
            //await _redditService.SendImageFromSubredditAsync(ctx, true);
        }

        [Command("anal")]
        public async Task AnalAsync(CommandContext ctx)
        {
            await _nsfwService.SendImageFromCategory(ctx, "anal");
        }

        [Command("pussy")]
        public async Task PussyAsync(CommandContext ctx)
        {
            await _nsfwService.SendImageFromCategory(ctx, "pussy");
        }
    }
}