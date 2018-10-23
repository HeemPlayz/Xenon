#region

using System.Collections.Generic;
using Discord;

#endregion

namespace Xenon.Core.Discord.Addons.Interactive.Paginator
{
    public class PaginatedMessage
    {
        public IEnumerable<EmbedBuilder> Pages { get; set; }

        public PaginatedAppearanceOptions Options { get; set; } = PaginatedAppearanceOptions.Default;
    }
}