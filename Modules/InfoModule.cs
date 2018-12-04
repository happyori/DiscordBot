using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
	[Name("Info")]
	[Group("info")]
    public class InfoModule : ModuleBase<SocketCommandContext>
    {

		/* !info square $num => $num^2 */
		[Name("Square [number]")]
		[Command("square"), Summary("Squares a number.")]
		public async Task Square([Summary("The number to square.")] int num)
		{
			await Context.Channel.SendMessageAsync($"{num}^2 = {Math.Pow(num, 2)}");
		}

		/* !info userinfo => Sans#DDDD */
			/* !info userinfo @Khiony => Khiony#DDDD */
			/* !info userinfo Khiony#DDDD => Khiony#DDDD */
			/* !info userinfo Khiony => Khiony#DDDD */
		/* !info whois $ID => Khiony#DDDD */
		[Name("Userinfo (user)")]
		[Command("userinfo"), Summary("Returns info about the current user, or the user parameter, if one is passed.")]
		[Alias("user", "whois")]
		public async Task UserInfo([Summary("The (optional) user to get info for")] IUser user = null)
		{
			var userInfo = user ?? Context.Client.CurrentUser;
			await ReplyAsync($"{userInfo.Username}#{userInfo.Discriminator}");
		}

		[Name("Userhierarchy (user)")]
		[Command("userhierarchy"), Alias("userH", "hierarchy")]
		[Summary("Returns a number for the hierarchy for the user, if no user specified uses the bot")]
		public async Task UserHierarchy([Summary("The (optional) user to get hierarchy for")] IUser user = null)
		{
			SocketGuildUser userInfo = (user as SocketGuildUser) ?? Context.Guild.GetUser(Context.Client.CurrentUser.Id);
			await ReplyAsync($"{userInfo.Username}#{userInfo.Discriminator}'s Hierarchy is : {userInfo.Hierarchy}");
		}
    }
}