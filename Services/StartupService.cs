using System;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Discord;
using Discord.Commands;
using Discord.WebSocket;


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
			string token = _config["token:discord"];
			if (string.IsNullOrWhiteSpace(token))
				throw new Exception("Please enter the bot's token into the 'config.json' file found in the application's root directory");
			
			await _client.LoginAsync(TokenType.Bot, token);
			await _client.StartAsync();

			await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
		}
    }
}