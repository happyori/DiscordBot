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
					return;
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
					return;
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

			[Name("Talk")]
			[Command("talk")]
			[Summary("Summons Sans to join you in voice chat and talk to you!")]
			public async Task Talk()
			{
				var channel = (Context.User as IVoiceState).VoiceChannel;
				if (channel == null)
				{
					await ReplyAsync("Am I a joke to you?");
					return;
				}

				await _service.JoinAudioAsync(Context.Guild, channel);
				await _service.SendAudioAsync(Context.Guild, Context.Channel, "Audio/sans.mp3");
			}
		}

		[Name("Roll [Number of Rolls]d[Sides of Dice](+-*/%)(Offset)")]
		[Command("roll"), Alias("r")]
		[Summary("Rolls a die for you.")]
		public async Task Roll(string command)
		{
			/* Command format example !r 2d20 + 10 */
			#region Prepare
			bool hasoffset = false;
			command = command.ToLower();
			command = command.Trim();
			char[] signs = new char[] { '+', '-', '*', '/' };
			string showstr = null;
			long rolls = 0;
			int sides = 0;
			long offset = 0;
			long sum = 0;

			var authorbuilder = new EmbedAuthorBuilder()
			{
				Name = Globals.msg.Author.Username,
				IconUrl = Globals.msg.Author.GetAvatarUrl(),
			};

			var footerbuilder = new EmbedFooterBuilder()
			{
				Text = "Powered by your's truly"
			};

			var builder = new EmbedBuilder()
			{
				Color = new Color(255, 0, 135),
				Description = "The rolls are:",
				Author = authorbuilder,
				Footer = footerbuilder,
			};

			if (!command.Contains('d'))
			{
				await UsageRollAsync();
				return;
			}

			if (command.Contains('*') ||
				command.Contains('-') ||
				command.Contains('+') ||
				command.Contains('/') ||
				command.Contains('%'))
				hasoffset = true;

			string[] split = command.Split('d', 2, StringSplitOptions.RemoveEmptyEntries);

			if (!long.TryParse(split[0], out rolls))
			{
				await UsageRollAsync();
				return;
			}

			long[] result = new long[rolls];

			string[] temp;
			char sign = '\0';
			if (hasoffset)
			{
				sign = split[1][split[1].IndexOfAny(signs)];
				switch (sign)
				{
					case '+':
						temp = split[1].Split('+', 2, StringSplitOptions.RemoveEmptyEntries);
						break;
					case '-':
						temp = split[1].Split('-', 2, StringSplitOptions.RemoveEmptyEntries);
						break;
					case '*':
						temp = split[1].Split('*', 2, StringSplitOptions.RemoveEmptyEntries);
						break;
					default :
						temp = split[1].Split('/', 2, StringSplitOptions.RemoveEmptyEntries);
						break;
				}

				if (!long.TryParse(temp[1], out offset))
				{
					await UsageRollAsync();
					return;
				}
			}
			else
			{
				temp = new string[1];
				temp[0] = split[1];
			}

			if (!int.TryParse(temp[0], out sides))
			{
				await UsageRollAsync();
				return;
			}

			#endregion

			Random rand = new Random();
			for (long i = 0; i < rolls; i++)
			{
				result[i] = rand.Next(1, sides);
				if (showstr == null)
					showstr = $"{result[i]}";
				else
					showstr = showstr.Insert(showstr.Length, $" + {result[i]}");
			}

			if (hasoffset)
				showstr = showstr.Insert(showstr.Length, $" {sign} {offset}");

			builder.AddField(x =>
			{
				x.Name = "Rolls:";
				x.Value = showstr;
				x.IsInline = true;
			});

			foreach (int res in result)
				sum += res;

			if (hasoffset)
				switch (sign)
				{
					case '+':
						sum += offset;
						break ;
					case '-':
						sum -= offset;
						break ;
					case '*':
						sum *= offset;
						break ;
					default :
						sum /= offset;
						break ;
				}
		
			builder.AddField(x =>
			{
				x.Name = "Result:";
				x.Value = $"**{sum}**";
				x.IsInline = false;
			});

			await ReplyAsync("", false, builder.Build());
		}

		private async Task UsageRollAsync(string dbg = null)
		{
			if (dbg != null)
				await ReplyAsync(dbg);
			var builder = new EmbedBuilder()
			{
				Color = new Color(38, 196, 255),
				Description = "Usage:"
			};
			builder.AddField(x =>
			{
				x.Name = "Usage:";
				x.Value = "!roll(!r) 1d20(+-/*)(10)";
				x.IsInline = true;
			});
			await ReplyAsync("", false, builder.Build());
		}
	}
}