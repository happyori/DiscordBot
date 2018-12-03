using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
	[Group("info")]
    public class InfoModule : ModuleBase
    {

		/* !info square $num => $num^2 */
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
		[Command("userinfo"), Summary("Returns info about the current user, or the user parameter, if one is passed.")]
		[Alias("user", "whois")]
		public async Task UserInfo([Summary("The (optional) user to get info for")] IUser user = null)
		{
			var userInfo = user ?? Context.Client.CurrentUser;
			await ReplyAsync($"{userInfo.Username}#{userInfo.Discriminator}");
		}
    }
}