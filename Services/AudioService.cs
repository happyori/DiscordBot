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

		public async Task<string> LeaveAudioAsync(IGuild guild)
		{
			IAudioClient client;
			if (ConnectedChannels.TryRemove(guild.Id, out client))
			{
				await client.StopAsync();
				return ("");
			}
			else
				return ("Am I a joke to you?");
		}

		public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, string path)
		{
			if (!File.Exists(path))
			{
				await channel.SendMessageAsync("File doesn't exist!");
				return ;
			}

			IAudioClient client;
			if (ConnectedChannels.TryGetValue(guild.Id, out client))
			{
				using (var ffmpeg = CreateProcess(path))
				using (var stream = client.CreatePCMStream(AudioApplication.Music))
				{
					try { await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream); }
					finally { await stream.FlushAsync(); }
				}
			}
		}

		private Process CreateProcess(string path)
			=> Process.Start(new ProcessStartInfo
			{
				FileName = "ffmpeg.exe",
				Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
				UseShellExecute = false,
				RedirectStandardOutput = true,
			});
    }
}