#region

using System;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Xenon.Services.External;

#endregion

namespace Xenon.Core
{
    public class HelpFormatter : BaseHelpFormatter
    {
        private readonly CommandContext _ctx;
        private readonly Server _server;
        private DiscordEmbedBuilder _embedBuilder;

        public HelpFormatter(CommandContext ctx) : base(ctx)
        {
            if (ctx.Guild != null)
                _server = ctx.CommandsNext.Services.GetService<DatabaseService>().GetObject<Server>(ctx.Guild.Id);
            _ctx = ctx;
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            var category = command.CustomAttributes.OfType<CommandCategoryAttribute>().FirstOrDefault()?.Category ??
                           command.Module.ModuleType.GetCustomAttributes(true).OfType<CommandCategoryAttribute>()
                               .FirstOrDefault()?.Category;
            if (!category.HasValue) return this;
            if (_server != null)
                if (_server.DisabledCategories.Contains(category.Value))
                    return this;

            _embedBuilder = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Purple)
                .WithTitle($"{command.Name.Humanize()} command help")
                .WithDescription($"{Formatter.Bold("Description")} ❯ {command.Description ?? "None"}" +
                                 $"\n{Formatter.Bold("Category")} ❯ {category.Value}")
                .AddField($"{(command.Overloads.Count == 1 ? "Usage" : "Usages")}", command.GetUsage());

            if (command.Aliases.Any())
                _embedBuilder.AddField("Aliases",
                    $"{string.Join(", ", command.Aliases.Select(x => $"{Formatter.InlineCode(x)}"))}");

            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            if (_embedBuilder == null)
            {
                _embedBuilder = new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Purple)
                    .WithTitle(
                        $"{_ctx.Guild?.CurrentMember.DisplayName ?? _ctx.Client.CurrentUser.Username} command manual")
                    .WithDescription(
                        $"Use {Formatter.InlineCode($"{_ctx.Prefix}help <command>")} to see more information about a specific command\n\n{Formatter.InlineCode("< >")} indicates a required parameter\n{Formatter.InlineCode("( )")} indicates an optional parameter");

                foreach (var value in Enum.GetValues(typeof(CommandCategory)))
                {
                    var category = (CommandCategory) value;
                    var commandList = (from subcommand in subcommands
                        let attribute =
                            subcommand.CustomAttributes.OfType<CommandCategoryAttribute>().FirstOrDefault()?.Category ??
                            subcommand.Module.ModuleType.GetCustomAttributes(true).OfType<CommandCategoryAttribute>()
                                .FirstOrDefault()?.Category
                        where attribute != null
                        where attribute == category
                        select Formatter.InlineCode(subcommand.Name)).ToList();

                    if (commandList.Any())
                        _embedBuilder.AddField($"{category.Humanize()}", string.Join(", ", commandList));
                }
            }
            else
            {
                _embedBuilder.AddField("Subcommands",
                    string.Join(", ", subcommands.Select(x => Formatter.InlineCode(x.Name))));
            }

            return this;
        }

        public override CommandHelpMessage Build()
        {
            if (_embedBuilder == null) return new CommandHelpMessage();
            return _embedBuilder == null ? new CommandHelpMessage() : new CommandHelpMessage(embed: _embedBuilder);
        }
    }
}