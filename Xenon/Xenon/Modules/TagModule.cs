#region

using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Sparrow.Platform.Posix;
using Sparrow.Utils;
using Xenon.Core;
using Xenon.Services.External;

#endregion

namespace Xenon.Modules
{
    [Group("tag")]
    [RequireGuild]
    public class TagModule : CommandModule
    {
        private readonly DatabaseService _databaseService;
        private readonly UtilService _utilService;
        private readonly InteractivityExtension _interactivity;

        public TagModule(DatabaseService databaseService, UtilService utilService, InteractivityExtension interactivity)
        {
            _databaseService = databaseService;
            _utilService = utilService;
            _interactivity = interactivity;
        }

        [GroupCommand]
        public async Task TagAsync(CommandContext ctx, [RemainingText] string tag)
        {
            var server = _databaseService.GetObject<Server>(ctx.Guild.Id);
            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Purple);
            if (server.Tags.TryGetValue(tag, out var specificTag))
            {
                var user = await ctx.Guild.GetMemberAsync(specificTag.AuthorId);
                embed.WithTitle($"{specificTag.Name}")
                    .WithDescription(specificTag.Message)
                    .WithFooter($"by {(user == null ? "invalid-user" : user.DisplayName)}")
                    .WithTimestamp(specificTag.TimeStamp);
            }
            else
            {
                var bestMatchingTags = server.Tags.OrderBy(pair => _utilService.CalculateDifference(tag, pair.Key))
                    .Take(3);
                embed.WithTitle("Tag not found")
                    .WithDescription($"Couldn't find a tag for {Formatter.InlineCode(tag)}");
                if (bestMatchingTags.Any())
                    embed.Description += string.Join("",
                        bestMatchingTags.Select(x => $"\n- {Formatter.InlineCode(x.Key)}"));
            }

            await ctx.RespondAsync(embed: embed);
        }

        [Command("add")]
        [Aliases("a")]
        public async Task AddTagAsync(CommandContext ctx, string name, [RemainingText] string message)
        {
            var server = _databaseService.GetObject<Server>(ctx.Guild.Id);

            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Purple);

            if (server.Tags.TryGetValue(name, out var tag))
            {
                CheckPermissions(ctx.Member, tag);
                tag.AuthorId = ctx.User.Id;
                tag.Name = name;
                tag.Message = message;
                embed.WithTitle("Tag updated")
                    .WithDescription($"Updated the message for the tag {Formatter.InlineCode(name)}");
            }
            else
            {
                tag = new Tag{AuthorId = ctx.User.Id, Name = name,Message = message, TimeStamp = DateTime.Now};
                embed.WithTitle("Tag added")
                    .WithDescription($"Added the tag {Formatter.InlineCode(tag.Name)}");
            }

            await ctx.RespondAsync(embed: embed);
            
            server.Tags[name] = tag;
            _databaseService.AddOrUpdateObject(server, server.Id);
        }

        [Command("make")]
        [Aliases("m")]
        public async Task MakeAsync(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Purple)
                .WithTitle("Make a new tag")
                .WithDescription($"What should be the name of the tag? (Type {Formatter.InlineCode("cancel")} to stop");
            await ctx.RespondAsync(embed: embed);
            var name = (await _interactivity.WaitForMessageAsync(x => x.Author.Id == ctx.User.Id)).Message.Content;
            if (string.Equals(name, "cancel", StringComparison.OrdinalIgnoreCase))
            {
                embed.WithTitle("Cancelled")
                    .WithDescription("Cancelled the tag creation");
                await ctx.RespondAsync(embed: embed);
                return;
            }

            embed.WithDescription(
                $"What should be the message of the tag? (Type {Formatter.InlineCode("cancel")} to stop");
            await ctx.RespondAsync(embed: embed);
            var message = (await _interactivity.WaitForMessageAsync(x => x.Author.Id == ctx.User.Id)).Message.Content;
            if (string.Equals(message, "cancel", StringComparison.OrdinalIgnoreCase))
            {
                embed.WithTitle("Cancelled")
                    .WithDescription("Cancelled the tag creation");
                await ctx.RespondAsync(embed: embed);
                return;
            }

            await AddTagAsync(ctx, name, message);
        }

        private void CheckPermissions(DiscordMember user, Tag tag)
        {
            var target = user.Guild.Members.FirstOrDefault(x => x.Id == tag.AuthorId);
            if (target == null) return;
            if (user.Id == tag.AuthorId) return;

            var targetUserRole = target.Roles.FirstOrDefault();
            var targetUserHierachy = targetUserRole == null ? 0 : targetUserRole.Position;
            var userRole = user.Roles.FirstOrDefault();
            var userHierachy = userRole == null ? 0 : userRole.Position;

            if (targetUserHierachy >= userHierachy)
                throw new HierachyException(
                    "This tag doesn't belong to you and you have not enough permissions to claim it");
        }
    }
}