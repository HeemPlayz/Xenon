#region

using System.Linq;
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
    [Group("tag")]
    public class TagModule : CommandModule
    {
        private readonly DatabaseService _databaseService;
        private readonly UtilService _utilService;

        public TagModule(DatabaseService databaseService, UtilService utilService)
        {
            _databaseService = databaseService;
            _utilService = utilService;
        }

        [GroupCommand]
        public async Task TagAsync(CommandContext ctx, [RemainingText] string tag)
        {
            var server = _databaseService.GetObject<Server>(ctx.Guild.Id);
            var embed = new DiscordEmbedBuilder();
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
                    .WithColor(DiscordColor.Purple)
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
        }
    }
}