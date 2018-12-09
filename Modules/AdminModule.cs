using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
	[Name("Admin")]
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
		[Name("Admin - Set")]
		[Group("set")]
		[RequireContext(ContextType.Guild)]
		public class Set : ModuleBase
		{
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
					return ;
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
					return ;
				}
				await (user.ModifyAsync(x =>
				{
					x.Nickname = null;
				}));
				await ReplyAsync($"{user.Mention} I changed your name back!");
			}
		}
		
		
    }
}