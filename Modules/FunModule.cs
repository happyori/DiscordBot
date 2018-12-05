using System;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections;
using DiscordBot.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace DiscordBot.Modules
{
	[Name("Fun")]
	[RequireContext(ContextType.Guild)]
	public class FunModule : ModuleBase<SocketCommandContext>
	{
		[Name("Fun - Audio")]
		public class AudioModule : ModuleBase
		{
			private readonly AudioService _service;
			public AudioModule(AudioService service)
			{
				_service = service;
			}

			[Name("Join [Channel Name]")]
			[Command("join"), Alias("j")]
			[Summary("Makes Sans Join the specified channel")]
			public async Task JoinbyName(string name)
			{
				IVoiceChannel chl = null;
				var channels = await Context.Guild.GetVoiceChannelsAsync();
				foreach (var channel in channels)
				{
					if (String.Equals(name, channel.Name))
						chl = channel;
				}
				if (chl == null)
				{
					await ReplyAsync($"I can't find channel -> {name}, Do I even have access to it?");
					return ;
				}
				await _service.JoinAudioAsync(Context.Guild, chl);
			}
			
			[Name("Join")]
			[Command("join"), Alias("j")]
			[Summary("Sans Joins to the channel you are in")]
			public async Task JoinbyUser()
			{
				var channel = (Context.User as IVoiceState).VoiceChannel;
				if (channel == null)
				{
					await ReplyAsync("Am I a joke to you?");
					return ;
				}

				await _service.JoinAudioAsync(Context.Guild, channel);
			}

			[Name("Leave")]
			[Command("leave"), Alias("l")]
			[Summary("Sans Leaves the server he is in")]
			public async Task Leave()
			{
				string response = await _service.LeaveAudioAsync(Context.Guild);
				if (!String.IsNullOrEmpty(response))
					await ReplyAsync(response);
			}
		}
	}
}