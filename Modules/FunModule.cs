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

		[Name("Join [Chat Name]")]
		[Command("join")]
		[Summary("joins the specified voice chat")]
		public async Task Join(string chatName)
		{
			var VChannels = Context.Guild.VoiceChannels;
			SocketVoiceChannel vcha = null;
			
			foreach (var channel in VChannels)
			{
				if (String.Compare(chatName, channel.Name) == 0)
					vcha = channel;
			}
			if (vcha == null)
			{
				await ReplyAsync($"Couldn't find {chatName} server, are you sure I have access to it?");
				return ;
			}

			var audioClient = await vcha.ConnectAsync();
			if (!ConnectedChannels.TryAdd(Context.Guild.Id, audioClient))
				await ReplyAsync($"Error -> Couldn't add audioClient to Dictionary!");
		}
		
		[Name("Leave")]
		[Command("leave"), Alias("l")]
		[Summary("Leaves current voice server.")]
		public async Task Leave()
		{
			IAudioClient client;
			if (ConnectedChannels.TryRemove(Context.Guild.Id, out client))
				await client.StopAsync();
			else
				await ReplyAsync($"Hey I am __not__ connected to any channel!");
		}
	}
}