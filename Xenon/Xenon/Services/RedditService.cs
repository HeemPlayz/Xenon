using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json.Linq;
using Xenon.Services.External;

namespace Xenon.Services
{
    public class RedditService
    {
        private readonly HttpClient _httpClient;
        private readonly ConfigurationService _configurationService;
        private readonly ImageService _imageService;

        public RedditService(HttpClient httpClient, ConfigurationService configurationService, ImageService imageService)
        {
            _httpClient = httpClient;
            _configurationService = configurationService;
            _imageService = imageService;
        }

        public async Task SendImageFromSubredditAsync(CommandContext ctx, string subreddit)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", $"{_configurationService.KsoftApiKey}");

            var image = await _imageService.ResolveImage(
                await _httpClient.GetStringAsync($"https://api.ksoft.si/meme/rand-reddit/{subreddit}"));
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