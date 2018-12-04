using System;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Services;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

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

		[Name("Kick [User]")]
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

		[Name("Ban [User]")]
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

		[Name("Ban [Amount of days] [User]")]
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

		[Name("Clean")]
		[Command("clean")]
		[Summary("Cleans 10 previous messages")]
		[RequireUserPermission(GuildPermission.ManageMessages)]
		public async Task Clean()
			=> await Clean(10);

		[Name("Clean [Number of messages]")]
		[Command("clean")]
		[Summary("Cleans specified amount of previous messages")]
		[RequireUserPermission(GuildPermission.ManageMessages)]
		public async Task Clean(int count)
		{
			var channel = Context.Guild.GetTextChannel(Context.Channel.Id);
			IEnumerable<IMessage> messages;
			const int delay = 5000;
			int i = 1;
			if (count > 100)
				i = count / 100;
			while (i > 0)
			{
				if (count < 100)
					messages = await Context.Channel.GetMessagesAsync(count + 1).FlattenAsync();
				else
					messages = await Context.Channel.GetMessagesAsync(100).FlattenAsync();
				await channel.DeleteMessagesAsync(messages);
				i--;
			}
			var m = await ReplyAsync($"The {count} of previous messages were deleted! __This message will be deleted in {delay / 1000} seconds.__");
			await Task.Delay(delay);
			await m.DeleteAsync();
		}
    }
}