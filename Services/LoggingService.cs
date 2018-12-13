using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
	public class LoggingService
	{
		private readonly DiscordSocketClient _client;
		private readonly CommandService _commands;

		private string _logDirectory { get; set; }
		private string _logFile => Path.Combine(_logDirectory, $"{DateTime.UtcNow.ToString("yyyy-MM-dd")}.txt");

		public LoggingService(
			DiscordSocketClient client,
			CommandService commands)
		{
			_logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");

			_client = client;
			_commands = commands;

			_client.Log += OnLoginAsync;
			_commands.Log += OnLoginAsync;
		}

		private Task OnLoginAsync(LogMessage msg)
		{
			if (!Directory.Exists(_logDirectory))
				Directory.CreateDirectory(_logDirectory);
			if (!File.Exists(_logFile))
				File.Create(_logFile).Dispose();

			string logText = $"{DateTime.UtcNow.ToString("hh:mm:ss")} [{msg.Severity}] {msg.Source} {msg.Exception?.ToString() ?? msg.Message}";
			File.AppendAllText(_logFile, logText + "\n");

			return (Console.Out.WriteLineAsync(logText));
		}
	}
}