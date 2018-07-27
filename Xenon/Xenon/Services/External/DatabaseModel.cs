#region

using System;
using System.Collections.Generic;
using DSharpPlus.Entities;
using Newtonsoft.Json;

#endregion

namespace Xenon.Services.External
{
    public class User
    {
        public string Username { get; set; }
        public string Id { get; set; }

        [JsonIgnore]
        public ulong UserId
        {
            get => ulong.Parse(Id);
            set => Id = $"{value}";
        }

        private string Biography { get; set; }

        [JsonIgnore]
        public string Bio
        {
            get => Biography ?? "None set";
            set => Biography = value;
        }
    }

    public class Server
    {
        public HashSet<ulong> Blacklist = new HashSet<ulong>();
        public ChannelBlockingType BlockingType = ChannelBlockingType.None;
        public HashSet<string> JoinMessages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> LeaveMessages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public bool LevelingState = true;
        public HashSet<string> LevelUpMessages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<ulong, ModLogItem> ModLog = new Dictionary<ulong, ModLogItem>();
        public bool NsfwState = true;
        public HashSet<string> Prefixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, Tag> Tags = new Dictionary<string, Tag>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<ulong, Userxp> Userxps = new Dictionary<ulong, Userxp>();
        public HashSet<ulong> Whitelist = new HashSet<ulong>();
        public string Name { get; set; }
        public string Id { get; set; }

        [JsonIgnore]
        public ulong ServerId
        {
            get => ulong.Parse(Id);
            set => Id = $"{value}";
        }

        public ulong? AnnounceChannelId { get; set; }
        public ulong? AutoroleId { get; set; }
        public ulong? LogChannelId { get; set; }
    }

    public class Userxp
    {
        private string Id { get; set; }

        [JsonIgnore]
        private ulong UserId
        {
            get => ulong.Parse(Id);
            set => Id = $"{value}";
        }

        public ulong Level { get; set; }
        public ulong Xp { get; set; }
    }

    public class ModLogItem
    {
        private string Id { get; set; }

        [JsonIgnore]
        public ulong LogId
        {
            get => ulong.Parse(Id);
            set => Id = $"{value}";
        }

        private string ResponsibleUser { get; set; }

        [JsonIgnore]
        public ulong ResponsibleUserId
        {
            get => ulong.Parse(ResponsibleUser);
            set => ResponsibleUser = $"{value}";
        }

        private string User { get; set; }

        [JsonIgnore]
        public ulong UserId
        {
            get => ulong.Parse(User);
            set => User = $"{value}";
        }

        public string Reason { get; set; }
        public AuditLogActionType ActionType { get; set; }
    }

    public class Tag
    {
        public string Name { get; set; }
        public string Message { get; set; }
        private string Author { get; set; }

        public ulong AuthorId
        {
            get => ulong.Parse(Author);
            set => Author = $"{value}";
        }

        public DateTime TimeStamp { get; set; }
    }

    public class Warn
    {
        public string Reason { get; set; }
        public ulong AuthorId { get; set; }
        public int WarnId { get; set; }
        public DateTime TimeStamp { get; set; }
    }

    public enum ChannelBlockingType
    {
        Whitelist,
        Blacklist,
        None
    }
}