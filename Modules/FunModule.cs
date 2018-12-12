using System;
using Discord;
using System.IO;
using System.Linq;
using Discord.Audio;
using Newtonsoft.Json;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections;
using DiscordBot.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;

namespace DiscordBot.Modules
{
	[Name("Fun")]
	[RequireContext(ContextType.Guild)]
	public class FunModule : ModuleBase<SocketCommandContext>
	{
		private readonly IConfigurationRoot _config;
		public FunModule(IConfigurationRoot config)
		{
			_config = config;
		}

		[Name("Say [Echo]")]
		[Command("say"), Alias("s")]
		[Summary("Echoes what's been said")]
		public async Task Say([Remainder] string msg)
		{
			var task = Globals.msg.DeleteAsync();
			await ReplyAsync(msg);
			task.Wait();
		}
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

		[Name("Roll [Number of Rolls] d [Sides of Dice] (+-*/) (Offset)")]
		[Command("roll"), Alias("r")]
		[Summary("Rolls a die for you.")]
		public async Task Roll([Summary(""), Remainder] string command)
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

			if (!long.TryParse(split[0], out rolls) || rolls == 0)
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
						temp = split[1].Split('+', 2);
						break;
					case '-':
						temp = split[1].Split('-', 2);
						break;
					case '*':
						temp = split[1].Split('*', 2);
						break;
					default:
						temp = split[1].Split('/', 2);
						break;
				}

				if (string.IsNullOrEmpty(temp[1]) || !long.TryParse(temp[1], out offset))
				{
					await UsageRollAsync();
					return;
				}

				if (sign == '/' && offset == 0)
				{
					await ReplyAsync($"It is truly unholy to divide by 0\n{Globals.msg.Author.Mention} should be ashamed of yourself!");
					return ;
				}
			}
			else
			{
				temp = new string[1];
				temp[0] = split[1];
			}

			if (!int.TryParse(temp[0], out sides) || sides == 0)
			{
				await UsageRollAsync();
				return;
			}

			#endregion

			Random rand = new Random();
			for (long i = 0; i < rolls; i++)
			{
				result[i] = rand.Next(1, sides + 1);
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
						break;
					case '-':
						sum -= offset;
						break;
					case '*':
						sum *= offset;
						break;
					default:
						sum /= offset;
						break;
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

			string prefix = _config["prefix"];

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
				Color = new Color(38, 196, 255),
				Author = authorbuilder,
				Footer = footerbuilder,
			};
			builder.AddField(x =>
			{
				x.Name = "Usage:";
				x.Value = $"{prefix}roll({prefix}r) 1d20(+-/*)(Number)";
				x.IsInline = true;
			});
			builder.AddField(x =>
			{
				x.Name = "Example:";
				x.Value = $"{prefix}r 1d20-1";
				x.IsInline = false;
			});
			await ReplyAsync("", false, builder.Build());
		}
	
		private class RespectsNum
		{
			public int Count { get; set; }
			public DateTime day { get; set; }

			public RespectsNum(int i)
			{
				Count = i;
				day = DateTime.Today;
			}
		}
	
		[Name("Pay Respects")]
		[Command("f")]
		[Summary("You pay respects!")]
		public async Task PayRespects()
		{
			RespectsNum nums;

			var user = Globals.msg.Author;

			string fileRes = Path.Combine(AppContext.BaseDirectory, "Respects.json");
			if (!File.Exists(fileRes))
				File.Create(fileRes);

			string fileNum = Path.Combine(AppContext.BaseDirectory, "Counts.json");
			if (!File.Exists(fileNum))
				File.Create(fileNum);

			string json = File.ReadAllText(fileRes);
			string num = File.ReadAllText(fileNum);

			string filler;

			Dictionary<string, bool> respects = JsonConvert.DeserializeObject<Dictionary<string, bool>>(json);
			nums = JsonConvert.DeserializeObject<RespectsNum>(num);

			if (respects == null)
				respects = new Dictionary<string, bool>();
			if (nums == null)
				nums = new RespectsNum(0);

			if (nums.day != DateTime.Today)
				ResetRespects(respects, nums);

			if (!respects.ContainsKey(user.Username))
				respects.Add(user.Username, false);
			if (respects[user.Username])
				filler = $"**{user.Username}** already paid their respects today!";
			else
			{
				filler = $"**{user.Username}** has paid their respects";
				nums.Count++;
				respects[user.Username] = true;
			}

			var footerBuilder = new EmbedFooterBuilder()
			{
				Text = $"Total Counts = {nums.Count}"
			};

			var authorbuilder = new EmbedAuthorBuilder()
			{
				Name = Globals.msg.Author.Username,
				IconUrl = Globals.msg.Author.GetAvatarUrl(),
			};

			var builder = new EmbedBuilder()
			{
				Color = new Color(255, 0, 135),
				Description = "Respects",
				Author = authorbuilder,
				Footer = footerBuilder,
			};

			builder.AddField(x =>
			{
				x.Name = "Your Respect";
				x.Value = filler;
			});

			await ReplyAsync("", false, builder.Build());

			json = JsonConvert.SerializeObject(respects, Formatting.Indented);
			num = JsonConvert.SerializeObject(nums, Formatting.Indented);

			if (File.Exists(fileRes))
				await File.WriteAllTextAsync(fileRes, json);
			if (File.Exists(fileNum))
				await File.WriteAllTextAsync(fileNum, num);
		}

		private void ResetRespects(Dictionary<string, bool> respects, RespectsNum nums)
		{
			foreach (string key in respects.Keys.ToList())
				respects[key] = false;
			nums.day = DateTime.Today;
		}
	}
}