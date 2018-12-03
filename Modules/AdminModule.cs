using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
	[Name("AdminModule")]
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        [Command("say"), Alias("s")]
		[Summary("Echoes what's been said")]
		[RequireUserPermission(GuildPermission.Administrator)]
		public Task Say([Remainder] string msg)
			=> ReplyAsync(msg);

		[Group("set")]
		[RequireContext(ContextType.Guild)]
		public class Set : ModuleBase
		{
			[Command("nick"), Priority(1)]
			[Summary("Set your own nickname to the specified phrase.")]
			[RequireUserPermission(GuildPermission.ChangeNickname)]
			public Task Nick([Remainder] string name)
				=> Nick(Context.User as SocketGuildUser, name);
			[Command("nick"), Priority(0)]
			[Summary("Set specified user's nickname to the specified phrase.")]
			[RequireUserPermission(GuildPermission.ChangeNickname)]
			public async Task Nick(SocketGuildUser user, [Remainder] string name)
			{
				await user.ModifyAsync(x => x.Nickname = name);
				await ReplyAsync($"{user.Mention} I changed your name to **{name}**!");
			}
		}
    }
}