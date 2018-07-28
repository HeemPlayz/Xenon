#region

using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Xenon.Core;
using Xenon.Services.External;

#endregion

namespace Xenon.Modules
{
    [Group("biography")]
    [Aliases("bio")]
    public class BiographyModule : CommandModule
    {
        private readonly DatabaseService _databaseService;

        public BiographyModule(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [GroupCommand]
        public async Task BiographyAsync(CommandContext ctx)
        {
            await BiographyAsync(ctx, ctx.User);
        }

        [GroupCommand]
        public async Task BiographyAsync(CommandContext ctx, DiscordUser user)
        {
            var embed = new DiscordEmbedBuilder()
                .WithAuthor(user.Username, icon_url: user.AvatarUrl)
                .WithDescription(_databaseService.GetObject<User>(ctx.User.Id).Bio)
                .WithColor(DiscordColor.Purple)
                .WithTimestamp(DateTime.Now);
            await ctx.RespondAsync(embed: embed);
        }

        [GroupCommand]
        public async Task BiographyAsync(CommandContext ctx, [RemainingText] string text)
        {
            var user = _databaseService.GetObject<User>(ctx.User.Id);
            user.Bio = text;
            _databaseService.AddOrUpdateObject(user, ctx.User.Id);

            var embed = new DiscordEmbedBuilder()
                .WithTitle("Biography set")
                .WithDescription($"Set your biography to {Formatter.InlineCode(text)}")
                .WithColor(DiscordColor.Purple)
                .WithTimestamp(DateTime.Now);
            await ctx.RespondAsync(embed: embed);
        }
    }
}