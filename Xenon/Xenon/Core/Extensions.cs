#region

using System;

#endregion

namespace Xenon.Core
{
    public enum CommandCategory
    {
        Moderation,
        General,
        Nsfw,
        Settings,
        Fun
    }

    public class CommandCategoryAttribute : Attribute
    {
        public CommandCategoryAttribute(CommandCategory category)
        {
            Category = category;
        }

        public CommandCategory Category { get; }
    }
}