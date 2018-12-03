using System;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Services;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
	[Name("Moderator")]
	[RequireContext(ContextType.Guild)]
    public class ModeratorModule : ModuleBase<SocketCommandContext>
    {
		private readonly CommandService _commands;

		public ModeratorModule(CommandService commands)
		{
			_commands = commands;
		}

		[Command("kick")]
		[Summary("Kick a specified user.")]
		[RequireUserPermission(GuildPermission.KickMembers)]
		public async Task Kick([Remainder] SocketGuildUser user)
		{
			SocketGuildUser author = Context.Guild.GetUser(Globals.AuthorId);
			if (author.DiscriminatorValue == user.DiscriminatorValue)
			{
				await ReplyAsync($"I am sure you had a reason to try to kick yourself...");
				return ;
			}
			else if (author.Hierarchy <= user.Hierarchy)
			{
				await ReplyAsync($"The {user.Username}#{user.Discriminator} is superior or equal to you!");
				return ;
			}
			await ReplyAsync($"cy@ {user.Mention} :wave:");
			await user.KickAsync();
		}

		[Command("ban")]
		[Summary("Bans a specified user.")]
		[RequireUserPermission(GuildPermission.BanMembers)]
		public async Task Ban([Remainder] SocketGuildUser user)
		{
			SocketGuildUser author = Context.Guild.GetUser(Globals.AuthorId);
			if (author.DiscriminatorValue == user.DiscriminatorValue)
			{
				await ReplyAsync($"I am sure you had a reason to try to ban yourself...");
				return ;
			}
			else if (author.Hierarchy <= user.Hierarchy)
			{
				await ReplyAsync($"The {user.Username}#{user.Discriminator} is superior or equal to you!");
				return ;
			}
			await ReplyAsync($"The banhammer has fallen on you {user.Mention}, Goodbye and never be seen on our eye!");
			await Context.Guild.AddBanAsync(user.Id);
		}

		[Command("ban")]
		[Summary("Bans for specified amount of days the specified user.")]
		[RequireUserPermission(GuildPermission.BanMembers)]
		public async Task Ban(int days, [Remainder] SocketGuildUser user)
		{
			SocketGuildUser author = Context.Guild.GetUser(Globals.AuthorId);
			if (author.DiscriminatorValue == user.DiscriminatorValue)
			{
				await ReplyAsync($"I am sure you had a reason to try to ban yourself...");
				return ;
			}
			else if (author.Hierarchy <= user.Hierarchy)
			{
				await ReplyAsync($"The {user.Username}#{user.Discriminator} is superior or equal to you!");
				return ;
			}
			await ReplyAsync($"The banhammer has fallen on you {user.Mention}, but you have a chance to repel! Comeback in {days} days.");
			await Task.Delay(10000);
			await Context.Guild.AddBanAsync(user.Id, days);
		}
    }
}