#region

using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Lavalink;
using DSharpPlus.Net.Udp;

#endregion

namespace Xenon.Services.Music
{
    public class MusicService
    {
        private readonly InteractivityExtension _interactivityExtension;
        private readonly LavalinkConfiguration _lavalinkConfiguration;
        private readonly LavalinkExtension _lavalinkExtension;
        private readonly LavalinkNodeConnection _nodeConnections;

        public MusicService(LavalinkExtension lavalinkExtension, InteractivityExtension interactivityExtension)
        {
            _lavalinkExtension = lavalinkExtension;
            _interactivityExtension = interactivityExtension;
            _lavalinkConfiguration = new LavalinkConfiguration
            {
                Password = "pw",
                RestEndpoint = new ConnectionEndpoint {Hostname = "127.0.0.1", Port = 2333},
                SocketEndpoint = new ConnectionEndpoint {Hostname = "127.0.0.1", Port = 80}
            };
            _nodeConnections = _lavalinkExtension.ConnectAsync(_lavalinkConfiguration).GetAwaiter().GetResult();
        }

        public async Task SendAudioAsync(CommandContext ctx, string song)
        {
            var embed = new DiscordEmbedBuilder();

            var track = await _nodeConnections.GetTracksAsync(BuildSearch(song));

            if (!track.Any())
            {
                embed.WithTitle("Song not found")
                    .WithDescription($"Couldn't find a track for {Formatter.InlineCode(song)}")
                    .WithColor(DiscordColor.Purple);
                await ctx.RespondAsync(embed: embed);
                return;
            }

            var player = _nodeConnections.GetConnection(ctx.Guild);

            player.Play(track.First());
        }

        private Uri BuildSearch(string query)
        {
            return Uri.IsWellFormedUriString(query, UriKind.RelativeOrAbsolute)
                ? new Uri(query)
                : new Uri($"ytsearch ❯{query}");
        }

        private DiscordEmbedBuilder MakeNowPlaying(LavalinkTrack track)
        {
            return new DiscordEmbedBuilder()
                .WithTitle($"Now playing {track.Title}")
                .WithDescription($"Artist ❯ {track.Author}\nLength ❯ {track.Length.Minutes}.{track.Length.Seconds}")
                .WithColor(DiscordColor.Purple);
        }
    }
}