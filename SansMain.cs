using System;
using System.Threading.Tasks;
using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Sans
{
	class SansMain
	{
		private DiscordSocketClient _client;
		private DiscordSocketConfig _config;
		private CommandService _commands;
		private IServiceProvider _services;

		public static void Main(string[] args)
			=> new SansMain().MainAsync().GetAwaiter().GetResult();

		public async Task MainAsync()
		{
			_config = new DiscordSocketConfig { MessageCacheSize = 100 };
			_client = new DiscordSocketClient();
			_commands = new CommandService();

			string token = "NTE4ODcxNDU1NTAxNjQ3ODg1.DuXYxA.p5AjTuV5Wy5r2cCgNV1IK-vb9z0";

			_services = new ServiceCollection().BuildServiceProvider();

			_client.Log += Log;
			await InstallCommands();

			await _client.LoginAsync(TokenType.Bot, token);
			await _client.StartAsync();

			await Task.Delay(-1);
		}

		private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}

		private async Task InstallCommands()
		{
			_client.MessageReceived += HandleCommand;
			await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
		}
		
		private async Task HandleCommand(SocketMessage messageParam)
		{
			int argPos = 0;

			var message = messageParam as SocketUserMessage;
			if (message == null)
				return ;
			
			if (!(message.HasCharPrefix('!', ref argPos)) ||
					message.HasMentionPrefix(_client.CurrentUser, ref argPos))
				return ;

			var context = new CommandContext(_client, message);

			var result = await _commands.ExecuteAsync(context, argPos, _services);
			if (!result.IsSuccess)
				await context.Channel.SendMessageAsync(result.ErrorReason);
		}
	}
}
