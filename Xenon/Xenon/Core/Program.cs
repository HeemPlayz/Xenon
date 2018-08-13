namespace Xenon.Core
{
    public class Program
    {
        private static void Main()
        {
            new DiscordBot().InitializeAsync().Wait();
        }
    }
}