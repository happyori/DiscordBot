using System;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using DiscordBot;

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
				return ;
			if (msg.Author.Id == _client.CurrentUser.Id)
				return ;

			Globals.AuthorId = msg.Author.Id;

			var context = new SocketCommandContext(_client, msg);

			int argPos = 0;
			if (msg.HasStringPrefix(_config["prefix"], ref argPos) || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
			{
				var result = await _commands.ExecuteAsync(context, argPos, _provider);

				if (!result.IsSuccess)
					await context.Channel.SendMessageAsync(result.ToString());
			}
		}

    }
}