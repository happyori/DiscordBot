using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Services;

namespace DiscordBot
{
	public static class Globals
	{
		public static ulong AuthorId { get; set; }
		public static SocketMessage msg { get; set; }
	}
	public class Startup
	{
		public IConfigurationRoot Configuration { get; }
		public Startup(string[] args)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(AppContext.BaseDirectory)
				.AddJsonFile("config.json");
			Configuration = builder.Build();
		}

		public static async Task RunAsync(string[] args)
		{
			var startup = new Startup(args);
			await startup.RunAsync();
		}

		public async Task RunAsync()
		{
			var services = new ServiceCollection();
			ConfigureServices(services);

			var provider = services.BuildServiceProvider();
			provider.GetRequiredService<LoggingService>();
			provider.GetRequiredService<CommandHandler>();
			provider.GetRequiredService<AudioService>();

			await provider.GetRequiredService<StartupService>().StartAsync();
			await Task.Delay(-1);
		}

		private void ConfigureServices(ServiceCollection services)
		{
			services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
			{
				LogLevel = LogSeverity.Verbose,
				MessageCacheSize = 1000
			}))
			.AddSingleton(new CommandService(new CommandServiceConfig
			{
				LogLevel = LogSeverity.Verbose,
				DefaultRunMode = RunMode.Async,
				CaseSensitiveCommands = false
			}))
			.AddSingleton<StartupService>()
			.AddSingleton<LoggingService>()
			.AddSingleton<AudioService>()
			.AddSingleton<CommandHandler>()
			.AddSingleton<Random>()
			.AddSingleton(Configuration);
		}
	}
}