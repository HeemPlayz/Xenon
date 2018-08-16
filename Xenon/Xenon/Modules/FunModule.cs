using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Discord;
using Discord.Commands;
using GiphyDotNet.Manager;
using GiphyDotNet.Model.Parameters;
using Newtonsoft.Json.Linq;
using Xenon.Core;
using Xenon.Services;

namespace Xenon.Modules
{
    [CommandCategory(CommandCategory.Fun)]
    [CheckState]
    public class FunModule : CommandBase
    {
        private readonly Configuration _configuration;
        private readonly Giphy _giphy;
        private readonly HttpClient _http;
        private readonly Random _random;

        public FunModule(HttpClient http, Random random, Configuration configuration, Giphy giphy)
        {
            _http = http;
            _random = random;
            _configuration = configuration;
            _giphy = giphy;
        }

        [Command("dog")]
        [Alias("woof")]
        [Summary("Sends a random dog image")]
        public async Task DogAsync()
        {
            string url;
            do
            {
                url = $"http://random.dog/{await _http.GetStringAsync("https://random.dog/woof")}";
            } while (url.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
                     url.EndsWith(".webm", StringComparison.OrdinalIgnoreCase));

            Console.WriteLine(url);
            var embed = new EmbedBuilder()
                .WithImageUrl(url);
            await ReplyEmbedAsync(embed);
        }

        [Command("fox")]
        [Summary("Sends a random fox image")]
        public async Task FoxAsync()
        {
            var url = $"{JObject.Parse(await _http.GetStringAsync("https://randomfox.ca/floof/"))["image"]}";

            var embed = new EmbedBuilder()
                .WithImageUrl(url);

            await ReplyEmbedAsync(embed);
        }

        [Command("8ball")]
        [Summary("8Ball will answer your question!")]
        public async Task EightballAsync([Remainder] string question)
        {
            var data = JObject.Parse(await _http.GetStringAsync("https://nekos.life/api/v2/8ball"));

            var embed = new EmbedBuilder()
                .WithTitle("8Ball has spoken")
                .WithDescription($"Question ❯ {question}\n\n8Ball's answer ❯ {data["response"]}")
                .WithThumbnailUrl($"{data["url"]}");

            await ReplyEmbedAsync(embed);
        }

        [Command("joke")]
        [Summary("Tells you a random joke :p")]
        public async Task JokeAsync()
        {
            var joke = $"{JObject.Parse(await _http.GetStringAsync("http://api.yomomma.info/"))["joke"]}";
            await ReplyAsync(joke);
        }

        [Command("lenny")]
        [Summary("Sends you a nice lenny ( ͡° ͜ʖ ͡°)")]
        public async Task LennyAsync()
        {
            var lennys = new[]
            {
                "( ͡° ͜ʖ ͡°)", "(☭ ͜ʖ ☭)", "(ᴗ ͜ʖ ᴗ)", "( ° ͜ʖ °)", "( ͡◉ ͜ʖ ͡◉)", "( ͡☉ ͜ʖ ͡☉)", "( ͡° ͜ʖ ͡°)>⌐■-■",
                "<:::::[]=¤ (▀̿̿Ĺ̯̿̿▀̿ ̿)", "( ͡ಥ ͜ʖ ͡ಥ)", "( ͡º ͜ʖ ͡º )", "( ͡ಠ ʖ̯ ͡ಠ)", "ᕦ( ͡°╭͜ʖ╮͡° )ᕤ", "( ♥ ͜ʖ ♥)",
                "(つ ♡ ͜ʖ ♡)つ", "✩°｡⋆⸜(▀̿Ĺ̯▀̿ ̿)", "⤜(ʘ_ʘ)⤏", "¯\\_ツ_/¯", "ಠ_ಠ", "ʢ◉ᴥ◉ʡ", "^‿^", "(づ◔ ͜ʖ◔)づ", "⤜(ʘ_ʘ)⤏",
                "☞   ͜ʖ  ☞", "ᗒ ͟ʖᗕ", "/͠-. ͝-\\", "(´• ᴥ •`)", "(╯￢ ᗝ￢ ）╯︵ ┻━┻", "ᕦ(・ᨎ・)ᕥ", "◕ ε ◕", "【$ ³$】",
                "(╭☞T ε T)╭☞"
            };
            await ReplyAsync(lennys[_random.Next(lennys.Length)]);
        }

        [Command("meme")]
        [Summary("Sends you a nice meme")]
        public async Task MemeAsync()
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Token", $"{_configuration.KsoftApiKey}");
            JObject data;
            do
            {
                data = JObject.Parse(await _http.GetStringAsync("https://api.ksoft.si/meme/random-meme"));
            } while (data.Value<bool>("nsfw"));

            var embed = new EmbedBuilder()
                .WithTitle($"{data["title"]}")
                .WithUrl($"{data["source"]}")
                .WithImageUrl($"{data["image_url"]}");

            await ReplyEmbedAsync(embed);

            _http.DefaultRequestHeaders.Authorization = null;
        }

        [Command("out")]
        [Alias("door")]
        [Summary("Shows someone the door")]
        public async Task OutAsync(IUser user)
        {
            await ReplyEmbedAsync(null, $"{user.Mention}  :point_right::skin-tone-1:  :door:");
        }

        [Command("say")]
        [Summary("Says your text!")]
        public async Task SayAsync([Remainder] string text)
        {
            await ReplyEmbedAsync(null, text);
        }

        [Command("lmgtfy")]
        [Alias("showgoogle", "sg", "showg", "sgoogle")]
        [Summary("Shows a dummy how to google")]
        public async Task ShowGoogleAsync([Remainder] string query)
        {
            var url = $"http://lmgtfy.com/?q={HttpUtility.UrlEncode(query)}";
            await ReplyAsync(url);
        }

        [Group("gif")]
        [Alias("giphy", "g")]
        [CommandCategory(CommandCategory.Fun)]
        [Summary("Sends a random gif or one with your tag")]
        [CheckNsfw]
        public class GiphyModule : CommandBase
        {
            private readonly Giphy _giphy;
            private readonly Random _random;

            public GiphyModule(Giphy giphy, Random random)
            {
                _giphy = giphy;
                _random = random;
            }

            [Command("")]
            [Summary("Sends a random gif")]
            public async Task GifAsync()
            {
                var gif = await _giphy.RandomGif(new RandomParameter());
                var embed = new EmbedBuilder()
                    .WithImageUrl(gif.Data.ImageUrl);

                await ReplyEmbedAsync(embed);
            }

            [Command("")]
            [Summary("Searches a gif from your query")]
            public async Task GifAsync([Remainder] string query)
            {
                var gif = await _giphy.GifSearch(new SearchParameter {Query = query});
                var embed = new EmbedBuilder();
                if (!gif.Data.Any())
                    embed.WithTitle("No gif found")
                        .WithDescription($"Couldn't find any gif for {query.InlineCode()}");
                else
                    embed.WithImageUrl(gif.Data[_random.Next(gif.Data.Length)].Images.Original.Url);

                await ReplyEmbedAsync(embed);
            }
        }
    }
}