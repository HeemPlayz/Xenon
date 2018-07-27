namespace Xenon.Core
{
    public enum CommandCategory
    {
        Moderation,
        General,
        Nsfw
    }

    public class CommandCategoryAttribute
    {
        public CommandCategoryAttribute(CommandCategory category)
        {
            Category = category;
        }

        public CommandCategory Category { get; set; }
    }
}