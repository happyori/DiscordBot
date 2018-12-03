using System;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

public class Say : ModuleBase 
{
	/* !say hello => hello */
	[Command("say"), Summary("Echoes a message.")]
	public async Task SayCommand([Remainder, Summary("The text to echo")] string echo)
	{
		await ReplyAsync(echo);
	}
}

[Group("info")]
public class Info : ModuleBase
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

[Group("admin")]
[RequireUserPermission(GuildPermission.Administrator)]
public class Admin : ModuleBase
{
	[Group("clean")]
	public class CleanModule : ModuleBase
	{
		/* !admin clean 10 */
		[Command]
		public async Task Default(int count = 10) => Messages(count);
		/* !admin clean messages 10 */
		[Command("messages")]
		public async Task Messages(int count = 10)
		{
			
		}
	}
}
