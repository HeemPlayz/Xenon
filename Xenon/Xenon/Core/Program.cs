#region

using Xenon.Core;

#endregion

namespace Xenon
{
    internal class Program
    {
        private static void Main()
        {
            new DiscordBot().InitializeAsync().Wait();
        }
    }
}