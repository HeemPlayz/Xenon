#region

using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using GiphyDotNet.Manager;
using GiphyDotNet.Model.Parameters;
using Newtonsoft.Json.Linq;
using Xenon.Core;
using Xenon.Services.External;

#endregion

namespace Xenon.Modules
{
    [CommandCategory(CommandCategory.Fun)]
    public class FunModule : CommandModule
    {
        private readonly ConfigurationService _configurationService;
        private readonly Giphy _giphyClient;
        private readonly HttpClient _httpClient;
        private readonly Random _random;

        public FunModule(HttpClient httpClient, ConfigurationService configurationService, Random random)
        {
            _httpClient = httpClient;
            _configurationService = configurationService;
            _random = random;
            _giphyClient = new Giphy(_configurationService.GiphyApiKey);
        }

        [Command("dog")]
        [Aliases("woof")]
        [Description("Sends a random dog image")]
        public async Task DogAsync(CommandContext ctx)
        {
            string url;
            do
            {
                url = $"http://random.dog/{await _httpClient.GetStringAsync("https://random.dog/woof")}";
            } while (url.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
                     url.EndsWith(".webm", StringComparison.OrdinalIgnoreCase));

            Console.WriteLine(url);
            var embed = new DiscordEmbedBuilder()
                .WithImageUrl(url)
                .WithColor(DiscordColor.Purple);
            await ctx.RespondAsync(embed: embed);
        }

        [Command("fox")]
        public async Task FoxAsync(CommandContext ctx)
        {
            var url = $"{JObject.Parse(await _httpClient.GetStringAsync("https://randomfox.ca/floof/"))["image"]}";

            var embed = new DiscordEmbedBuilder()
                .WithImageUrl(url)
                .WithColor(DiscordColor.Purple);

            await ctx.RespondAsync(embed: embed);
        }

        [Command("8ball")]
        [CheckParameters]
        public async Task EightballAsync(CommandContext ctx, [RemainingText] string question)
        {
            var data = JObject.Parse(await _httpClient.GetStringAsync("https://nekos.life/api/v2/8ball"));

            var embed = new DiscordEmbedBuilder()
                .WithTitle("8Ball has spoken")
                .WithDescription($"Question ❯ {question}\n\n8Ball's answer ❯ {data["response"]}")
                .WithThumbnailUrl($"{data["url"]}")
                .WithColor(DiscordColor.Purple);

            await ctx.RespondAsync(embed: embed);
        }

        [Command("joke")]
        public async Task JokeAsync(CommandContext ctx)
        {
            var joke = $"{JObject.Parse(await _httpClient.GetStringAsync("http://api.yomomma.info/"))["joke"]}";
            await ctx.RespondAsync(joke);
        }

        [Command("lenny")]
        public async Task LennyAsync(CommandContext ctx)
        {
            var lennys = new[]
            {
                "( ͡° ͜ʖ ͡°)", "(☭ ͜ʖ ☭)", "(ᴗ ͜ʖ ᴗ)", "( ° ͜ʖ °)", "( ͡◉ ͜ʖ ͡◉)", "( ͡☉ ͜ʖ ͡☉)", "( ͡° ͜ʖ ͡°)>⌐■-■",
                "<:::::[]=¤ (▀̿̿Ĺ̯̿̿▀̿ ̿)", "( ͡ಥ ͜ʖ ͡ಥ)", "( ͡º ͜ʖ ͡º )", "( ͡ಠ ʖ̯ ͡ಠ)", "ᕦ( ͡°╭͜ʖ╮͡° )ᕤ", "( ♥ ͜ʖ ♥)",
                "(つ ♡ ͜ʖ ♡)つ", "✩°｡⋆⸜(▀̿Ĺ̯▀̿ ̿)", "⤜(ʘ_ʘ)⤏", "¯\\_ツ_/¯", "ಠ_ಠ", "ʢ◉ᴥ◉ʡ", "^‿^", "(づ◔ ͜ʖ◔)づ", "⤜(ʘ_ʘ)⤏",
                "☞   ͜ʖ  ☞", "ᗒ ͟ʖᗕ", "/͠-. ͝-\\", "(´• ᴥ •`)", "(╯￢ ᗝ￢ ）╯︵ ┻━┻", "ᕦ(・ᨎ・)ᕥ", "◕ ε ◕", "【$ ³$】",
                "(╭☞T ε T)╭☞"
            };
            await ctx.RespondAsync(lennys[_random.Next(0, lennys.Length - 1)]);
        }

        [Command("meme")]
        public async Task MemeAsync(CommandContext ctx)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Token", $"{_configurationService.KsoftApiKey}");
            JObject data;
            do
            {
                data = JObject.Parse(await _httpClient.GetStringAsync("https://api.ksoft.si/meme/random-meme"));
            } while (data.Value<bool>("nsfw"));

            var embed = new DiscordEmbedBuilder()
                .WithTitle($"{data["title"]}")
                .WithUrl($"{data["source"]}")
                .WithImageUrl($"{data["image_url"]}")
                .WithColor(DiscordColor.Purple);

            await ctx.RespondAsync(embed: embed);

            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        [Command("out")]
        [Aliases("door")]
        public async Task OutAsync(CommandContext ctx, DiscordUser user)
        {
            var embed = new DiscordEmbedBuilder()
                .WithDescription($"{user.Mention}  :point_right::skin-tone-1:  :door:")
                .WithColor(DiscordColor.Purple);
            await ctx.RespondAsync(embed: embed);
        }

        [Command("say")]
        [Aliases("s")]
        public async Task SayAsync(CommandContext ctx, [RemainingText] string message)
        {
            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Purple)
                .WithDescription(message);

            await ctx.RespondAsync(embed: embed);
        }

        [Command("lmgtfy")]
        [Aliases("showgoogle", "sg", "showg", "sgoogle")]
        [CheckParameters]
        public async Task ShowGoogleAsync(CommandContext ctx, [RemainingText] string query)
        {
            var url = $"http://lmgtfy.com/?q={HttpUtility.UrlEncode(query)}";
            await ctx.RespondAsync(url);
        }

        [Command("gif")]
        public async Task GifAsync(CommandContext ctx)
        {
            var gif = await _giphyClient.RandomGif(new RandomParameter());
            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Purple)
                .WithImageUrl(gif.Data.ImageUrl);

            await ctx.RespondAsync(embed: embed);
        }

        [Command("gif")]
        public async Task GifAsync(CommandContext ctx, [RemainingText] string query)
        {
            var gif = await _giphyClient.GifSearch(new SearchParameter {Query = query});
            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Purple);
            if (!gif.Data.Any())
                embed.WithTitle("No gif found")
                    .WithDescription($"Couldn't find any gif for {Formatter.InlineCode(query)}");
            else
                embed.WithImageUrl(gif.Data[_random.Next(gif.Data.Length)].Images.Original.Url);

            await ctx.RespondAsync(embed: embed);
        }
    }
}