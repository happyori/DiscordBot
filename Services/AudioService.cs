using Discord;
using System.IO;
using Discord.Audio;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace DiscordBot.Services
{
    public class AudioService
    {
		private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();

		public async Task JoinAudioAsync(IGuild guild, IVoiceChannel channel)
		{
			IAudioClient client;
			if (ConnectedChannels.TryGetValue(guild.Id, out client))
				return ;
			if (channel.GuildId != guild.Id)
				return ;
			
			var audioClient = await channel.ConnectAsync();

			ConnectedChannels.TryAdd(guild.Id, audioClient);
		}

		public async Task LeaveAudioAsync(IGuild guild)
		{
			IAudioClient client;
			if (ConnectedChannels.TryRemove(guild.Id, out client))
				await client.StopAsync();
		}
    }
}