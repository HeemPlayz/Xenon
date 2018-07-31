#region

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Xenon.Core;
using Xenon.Services.External;

#endregion

namespace Xenon.Services
{
    public class RedditService
    {
        private readonly ConfigurationService _configurationService;
        private readonly HttpClient _httpClient;

        public RedditService(HttpClient httpClient, ConfigurationService configurationService)
        {
            _httpClient = httpClient;
            _configurationService = configurationService;
        }

        public async Task SendImageFromSubredditAsync(CommandContext ctx, bool gifs)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Token", $"{_configurationService.KsoftApiKey}");

            var image = JsonConvert.DeserializeObject<Models.KsoftImage>(
                await _httpClient.GetStringAsync(
                    $"https://api.ksoft.si/meme/random-nsfw?gifs={(gifs ? "yes" : "no")}"));
            Console.WriteLine(image.ImageUrl);
            var embed = new DiscordEmbedBuilder()
                .WithTitle(image.Title)
                .WithImageUrl(image.ImageUrl)
                .WithUrl(image.Source)
                .WithFooter($"by {image.Subreddit}")
                .WithColor(DiscordColor.Purple);

            await ctx.RespondAsync(embed: embed);

            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}