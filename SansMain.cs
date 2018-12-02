using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Sans
{
	class SansMain
	{
		public static void Main(string[] args)
			=> new SansMain().MainAsync().GetAwaiter().GetResult();

		public async Task MainAsync()
		{
			var client = new DiscordSocketClient();

			client.Log += Log;
			client.MessageReceived += MessageReceived;

			string token = "NTE4ODcxNDU1NTAxNjQ3ODg1.DuXYxA.p5AjTuV5Wy5r2cCgNV1IK-vb9z0";
			await client.LoginAsync(TokenType.Bot, token);
			await client.StartAsync();

			await Task.Delay(-1);
		}

		private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}
		
		private async Task MessageReceived(SocketMessage message)
		{
			if (message.Content == "!ping")
				await message.Channel.SendMessageAsync("Pong!");
		}
	}
}
