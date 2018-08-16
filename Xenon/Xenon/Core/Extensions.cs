﻿#region

using System;
using Discord.Commands;
using Discord.WebSocket;
using Xenon.Services;

#endregion

namespace Xenon.Core
{
    public enum CommandCategory
    {
        Moderation,
        [CannotDisable] General,
        Nsfw,
        [CannotDisable] Settings,
        Fun,
        [CannotDisable] BotOwner,
        Tools
    }

    public class CommandCategoryAttribute : Attribute
    {
        public CommandCategoryAttribute(CommandCategory category)
        {
            Category = category;
        }

        public CommandCategory Category { get; }
    }

    public class ExecutionObject
    {
        public Server Server;
    }

    public enum ColorType
    {
        Random,
        Normal
    }

    public enum ServerSettings
    {
        LeaveMessage,
        JoinMessage,
        Hastebin
    }

    public class EvaluateObject
    {
        public ShardedCommandContext Context { get; set; }
        public DiscordShardedClient Client { get; set; }
        public Server Server { get; set; }
        public DatabaseService Database { get; set; }
        public Random Random { get; set; }
    }
}