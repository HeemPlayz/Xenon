using System;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MoreLinq;
using Newtonsoft.Json.Linq;
using Xenon.Core;
using Xenon.Services;

namespace Xenon.Modules
{
    [RequireNsfw]
    public class NsfwModule : CommandModule
    {
        private readonly HttpClient _httpClient;
        private readonly Random _random;
        private readonly RedditService _redditService;

        public NsfwModule(HttpClient httpClient, Random random, RedditService redditService)
        {
            _httpClient = httpClient;
            _random = random;
            _redditService = redditService;
        }

        [Command("ass")]
        public async Task AssAsync(CommandContext ctx)
        {
            var random = _random.Next(6012);
            var data = JArray.Parse(await _httpClient.GetStringAsync($"http://api.obutts.ru/butts/{random}")).First;
            var embed = new DiscordEmbedBuilder()
                .WithImageUrl($"http://media.obutts.ru/{data["preview"]}")
                .WithFooter($"Ass #{random}")
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
                .WithFooter($"Boobs #{random}")
                .WithColor(DiscordColor.Purple);

            await ctx.RespondAsync(embed: embed);
        }

        [Command("hentai")]
        public async Task HentaiAsync(CommandContext ctx)
        {
            var url = $"{JObject.Parse(await _httpClient.GetStringAsync($"https://nekos.life/api/v2/img/Random_hentai_gif"))["url"]}";
            var embed = new DiscordEmbedBuilder()
                .WithImageUrl(url)
                .WithColor(DiscordColor.Purple);
            await ctx.RespondAsync(embed: embed);
        }

        [Command("nude")]
        public async Task NudeAsync(CommandContext ctx)
        {
            var randoms = new[] {"GoneWild", "GoneWild30Plus", "GWNerdy", "NSFW", "Nudes", "RealGirls", "BustyPetite", "Amateur", "PetiteGoneWild", "AsiansGoneWild", "adorableporn", "ass", "LegalTeens", "milf", "HappyEmbarrassedGirls", "OnOff", "Boobies", "pawg", "collegesluts", "NSFW_Snapchat", "asstastic", "palegirls", "GWCouples", "StraightGirlsPlaying", "juicyasians"};
             await _redditService.SendImageFromSubredditAsync(ctx, randoms[_random.Next(randoms.Length)]);
         }
 
         [Command("nudegif")]
         public async Task NudeGifAsync(CommandContext ctx)
         {
             var randoms = new[] {"NSFW_GIF", "porn_gifs", "cumsluts", "holdthemoan", "GirlsFinishingTheJob", "porninfifteenseconds", "TittyDrop", "60fpsporn", "nsfwhardcore"};
             await _redditService.SendImageFromSubredditAsync(ctx, randoms[_random.Next(randoms.Length)]);
         }
     }
 }