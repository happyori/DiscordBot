using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
	[Name("Admin")]
	public class AdminModule : ModuleBase<SocketCommandContext>
	{

		private readonly DiscordSocketClient _client;

		public AdminModule(DiscordSocketClient discord)
		{
			_client = discord;
		}
		[Name("Admin - Set")]
		[Group("set")]
		[RequireContext(ContextType.Guild)]
		public class Set : ModuleBase
		{
			private readonly DiscordSocketClient _client;

			public Set(DiscordSocketClient discord)
			{
				_client = discord;
			}

			[Name("Game [Name]")]
			[Command("game")]
			[Summary("Sets the bot's game")]
			public async Task Game([Remainder] string name = null)
			{
				if (Globals.AuthorId != 203408658942394368)
				{
					await ReplyAsync($"Only ${await Context.Guild.GetUserAsync(203408658942394368)} can do that!");
					return;
				}

				if (name == null)
					await _client.SetGameAsync("Undertail");
				else
					await _client.SetGameAsync(name);
			}


			[Name("Nick [Name]")]
			[Command("nickself")]
			[Summary("Set your own nickname to the specified phrase.")]
			[RequireUserPermission(GuildPermission.ChangeNickname)]
			public Task Nick([Remainder] string name)
				=> Nick(Context.User as SocketGuildUser, name);
			[Name("Nick [User] [Name]")]
			[Command("nick")]
			[Summary("Set specified user's nickname to the specified phrase.")]
			[RequireUserPermission(GuildPermission.ManageNicknames)]
			public async Task Nick(SocketGuildUser user, [Remainder] string name)
			{
				var botUser = await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id);
				if (user.Hierarchy > (botUser as SocketGuildUser).Hierarchy)
				{
					await ReplyAsync($"{user.Mention} is superior to this humble bot");
					return;
				}
				await (user.ModifyAsync(x =>
				{
					x.Nickname = name;
				}));
				await ReplyAsync($"{user.Mention} I changed your name to **{name}**!");
			}

			[Name("Nick [User]")]
			[Command("nick")]
			[Summary("Set Specified user's nickname back")]
			[RequireUserPermission(GuildPermission.ManageNicknames)]
			public async Task Renick(SocketGuildUser user)
			{
				var botUser = await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id);
				if (user.Hierarchy > (botUser as SocketGuildUser).Hierarchy)
				{
					await ReplyAsync($"{user.Mention} is superior to this humble bot");
					return;
				}
				await (user.ModifyAsync(x =>
				{
					x.Nickname = null;
				}));
				await ReplyAsync($"{user.Mention} I changed your name back!");
			}
		}
	
		[Name("Quit")]
		[Command("quit")]
		[Summary("Turns of the bot.")]
		[RequireUserPermission(GuildPermission.Administrator)]
		public async Task Quit()
		{
			if (Globals.AuthorId != 203408658942394368)
			{
				await ReplyAsync($"Only ${Context.Guild.GetUser(203408658942394368)} can do that!");
				return;
			}
			var guilds = _client.Guilds;

			foreach (var g in guilds)
			{
				string name = g.CurrentUser.Nickname ?? "Sans";
				if (!name.Contains("(unavailable)"))
				{
					name = name.Insert(name.Length, "(unavailable)");
					await g.CurrentUser.ModifyAsync(x =>
					{
						x.Nickname = name;
					});
				}
			}
			
			await ReplyAsync("Beep boop beeeee...");
			await Context.Client.StopAsync();
			System.Environment.Exit(0);
		}
	}
}