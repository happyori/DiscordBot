using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace DiscordBot.Services
{
	public class StartupService
	{
		private readonly DiscordSocketClient _client;
		private readonly CommandService _commands;
		private readonly IConfigurationRoot _config;

		public StartupService(
			DiscordSocketClient client,
			CommandService commands,
			IConfigurationRoot config)
		{
			_client = client;
			_commands = commands;
			_config = config;
		}

		public async Task StartAsync()
		{
			string token = _config["tokens:discord"];
			if (string.IsNullOrWhiteSpace(token))
				throw new Exception($"Please enter the bot's token into the 'config.json' file found in the application's root directory its currently is {token}");

			await _client.LoginAsync(TokenType.Bot, token);
			await _client.StartAsync();

			_client.Ready += ConfigureName;

			_client.ReactionAdded += OnReactionAdded;
			_client.ReactionRemoved += OnReactionRemoved;

			await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
		}

		private async Task ConfigureName()
		{
			var guilds = _client.Guilds;

			foreach (var guild in guilds)
			{
				string name = guild.CurrentUser.Nickname;
				if (name.Contains("(unavailable)"))
				{
					if (name != null)
					{
						name = name.Remove(name.Length - 13);
						await guild.CurrentUser.ModifyAsync(x =>
						{
							x.Nickname = name;
						});
					}
				}
			}

			await _client.SetGameAsync("Undertail");
		}

		private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> cacheable,
										 ISocketMessageChannel channel,
										 SocketReaction reaction)
		{
			Dictionary<ulong, Votes> dict;
			Votes votes;

			if (!(reaction.Emote.Name == "👍" || reaction.Emote.Name == "👎") || reaction.User.Value.IsBot)
				return ;

			string fileName = Path.Combine(AppContext.BaseDirectory, "Votes.json");
			if (!File.Exists(fileName))
				File.Create(fileName);
			
			string json = await File.ReadAllTextAsync(fileName);
			dict = JsonConvert.DeserializeObject<Dictionary<ulong, Votes>>(json);

			if (dict == null)
				dict = new Dictionary<ulong, Votes>();
			
			if (!dict.ContainsKey(reaction.MessageId))
				return;
			else
			{
				if (!dict.TryGetValue(reaction.MessageId, out votes))
					throw new Exception($"Couldn't get value with key {reaction.MessageId}");
				switch (reaction.Emote.Name)
				{
					case "👍":
						votes.upVote += 1;
						break;
					default:
						votes.downVote += 1;
						break;
				}
				dict.Remove(reaction.MessageId);
				dict.Add(reaction.MessageId, votes);
			}

			json = JsonConvert.SerializeObject(dict, Formatting.Indented);

			if (File.Exists(fileName))
				await File.WriteAllTextAsync(fileName, json);
		}

		private async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> cacheable,
										 ISocketMessageChannel channel,
										 SocketReaction reaction)
		{
			Dictionary<ulong, Votes> dict;
			Votes votes;

			if (!(reaction.Emote.Name == "👍" || reaction.Emote.Name == "👎") || reaction.User.Value.IsBot)
				return ;

			string fileName = Path.Combine(AppContext.BaseDirectory, "Votes.json");
			if (!File.Exists(fileName))
				File.Create(fileName);
			
			string json = await File.ReadAllTextAsync(fileName);
			dict = JsonConvert.DeserializeObject<Dictionary<ulong, Votes>>(json);

			if (dict == null)
				dict = new Dictionary<ulong, Votes>();
			
			if (!dict.ContainsKey(reaction.MessageId))
				return;
			else
			{
				if (!dict.TryGetValue(reaction.MessageId, out votes))
					throw new Exception($"Couldn't get value with key {reaction.MessageId}");
				switch (reaction.Emote.Name)
				{
					case "👍":
						votes.upVote -= 1;
						break;
					default:
						votes.downVote -= 1;
						break;
				}
				dict.Remove(reaction.MessageId);
				dict.Add(reaction.MessageId, votes);
			}

			json = JsonConvert.SerializeObject(dict, Formatting.Indented);

			if (File.Exists(fileName))
				await File.WriteAllTextAsync(fileName, json);
		}
	}
}