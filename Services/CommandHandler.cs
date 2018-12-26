using System;
using System.IO;
using System.Collections.Generic;
using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using DiscordBot;
using Newtonsoft.Json;

namespace DiscordBot.Services
{
	public class CommandHandler
	{
		private readonly DiscordSocketClient _client;
		private readonly CommandService _commands;
		private readonly IConfigurationRoot _config;
		private readonly IServiceProvider _provider;

		public CommandHandler(
			DiscordSocketClient client,
			CommandService commands,
			IConfigurationRoot config,
			IServiceProvider provider)
		{
			_client = client;
			_commands = commands;
			_config = config;
			_provider = provider;

			_client.MessageReceived += OnMessageReceive;
		}

		private async Task OnMessageReceive(SocketMessage s)
		{
			var msg = s as SocketUserMessage;
			if (msg == null)
				return;
			if (msg.Author.Id == _client.CurrentUser.Id)
				return;

			Globals.AuthorId = msg.Author.Id;
			Globals.Msg = msg;

			foreach (var attachment in msg.Attachments)
			{
				if (attachment == null)
					break;
				await AddVoteAsync(msg);
			}

			var context = new SocketCommandContext(_client, msg);

			int argPos = 0;
			if (msg.HasStringPrefix(_config["prefix"], ref argPos) || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
			{
				var result = await _commands.ExecuteAsync(context, argPos, _provider);

				if (!result.IsSuccess)
					await context.Channel.SendMessageAsync(result.ToString());
			}
		}


		private async Task AddVoteAsync(SocketUserMessage msg)
		{
			Dictionary<ulong, Votes> votes;
			Votes init;

			string fileName = Path.Combine(AppContext.BaseDirectory, "Votes.json");
			if (!File.Exists(fileName))
				File.Create(fileName);

			string json = await File.ReadAllTextAsync(fileName);
			votes = JsonConvert.DeserializeObject<Dictionary<ulong, Votes>>(json);

			if (votes == null)
			{
				votes = new Dictionary<ulong, Votes>();
			}
			
			if (!votes.ContainsKey(msg.Id))
			{
				init = new Votes(1, 1);
				votes.Add(msg.Id, init);
			}

			json = JsonConvert.SerializeObject(votes, Formatting.Indented);

			if (File.Exists(fileName))
				await File.WriteAllTextAsync(fileName, json);

			await msg.AddReactionAsync(new Emoji("👍"));
			await Task.Delay(2);
			await msg.AddReactionAsync(new Emoji("👎"));
			
		}
	}
	public class Votes
	{
		public long upVote;
		public long downVote;
		public Votes(long up, long down)
		{
			upVote = up;
			downVote = down;
		}
	}
}