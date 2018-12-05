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
		private readonly DiscordSocketClient _client;

		public ModeratorModule(CommandService commands, DiscordSocketClient client)
		{
			_commands = commands;
			_client = client;
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
			await user.KickAsync($"Kicked by {Context.Client.CurrentUser.Username}#{Context.Client.CurrentUser.Discriminator}.");
		}

		[Name("Kick [User] [Reason]")]
		[Command("kick")]
		[Summary("Kick a specified user with custom reason.")]
		[RequireUserPermission(GuildPermission.KickMembers)]
		public async Task Kick(SocketGuildUser user, [Remainder] string reason)
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
			await user.KickAsync(reason);
		}

		[Name("Moderator - Ban")]
		[Group("ban")]
		[RequireContext(ContextType.Guild)]

		public class Ban : ModuleBase<SocketCommandContext>
		{
			[Name("Ban [User]")]
			[Command]
			[Summary("Bans a specified user.")]
			[RequireUserPermission(GuildPermission.BanMembers)]
			public async Task BanUnlimited([Remainder] SocketGuildUser user)
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
				await Context.Guild.AddBanAsync(user.Id, 0, $"Banned by {Context.Client.CurrentUser.Username}#{Context.Client.CurrentUser.Discriminator} for eternity.");
			}

			[Name("Ban [Amount of days] [User]")]
			[Command]
			[Summary("Bans for specified amount of days the specified user.")]
			[RequireUserPermission(GuildPermission.BanMembers)]
			public async Task BanLimited(int days, [Remainder] SocketGuildUser user)
			{
				if (days < 0 || days > 7)
				{
					await ReplyAsync($"The range for the prune days is [0-7]!");
					return ;
				}

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
				await Task.Delay(5000);
				await Context.Guild.AddBanAsync(user.Id, days, $"Banned by {Context.Client.CurrentUser.Username}#{Context.Client.CurrentUser.Discriminator} for {days} days.");
			}

			[Name("Ban List")]
			[Command("list")]
			[Summary("Shows the list of all the banned users.")]
			public async Task ListBans()
			{
				var bans = await Context.Guild.GetBansAsync();
				var builder = new EmbedBuilder()
				{
					Color = new Color(86, 20, 127),
					Description = "These are the currently banned users"
				};

				if (bans.Count == 0)
				{
					builder.AddField(x =>
					{
						x.Name = "Bans";
						x.Value = "No users banned... yet";
						x.IsInline = false;
					});
				}

				foreach (var ban in bans)
				{
					string description = null;

					if (!String.IsNullOrWhiteSpace(ban.User.Username))
						description += $"{ban.User.Username}#{ban.User.Discriminator} : Reason -> {ban.Reason}\n";
					
					if (!String.IsNullOrWhiteSpace(description))
					{
						builder.AddField(x =>
						{
							x.Name = "Bans";
							x.Value = description;
							x.IsInline = false;
						});
					}
				}

				await ReplyAsync("", false, builder.Build());
			}
		}

		[Name("Unban [UserID]")]
		[Command("unban")]
		[Summary("Unbans a user that was previously banned.")]
		[RequireUserPermission(GuildPermission.BanMembers)]
		public async Task UnbanUser([Remainder] ulong userID)
		{
			var bans = await Context.Guild.GetBansAsync();
			IUser banneduser = null;

			foreach (var ban in bans)
			{
				if (ban.User.DiscriminatorValue == userID)
					banneduser = ban.User;
			}

			if (banneduser == null)
				await ReplyAsync($"I didn't find the User with id -> {userID} under the banhammer are you sure he is here?");
			else
			{
				await Context.Guild.RemoveBanAsync(banneduser);
				await ReplyAsync($"Ahhh there you are little {banneduser.Username} here you go.");
			}
		}

		[Name("Invite [Username] [UserDescriminator]")]
		[Command("invite")]
		[Summary("Invites a user to the server.")]
		[RequireUserPermission(GuildPermission.CreateInstantInvite)]
		public async Task InviteUser(string username, string descriminator)
			=> await InviteUser(username, descriminator, null, null);

		[Name("Invite [Username] [UserDescriminator] [Days Active]")]
		[Command("invite")]
		[Summary("Invites a user to the server.")]
		[RequireUserPermission(GuildPermission.CreateInstantInvite)]
		public async Task InviteUser(string username, string descriminator, int days)
			=> await InviteUser(username, descriminator, days, null);
		
		[Name("Invite [Username] [UserDescriminator] [Max Invite]")]
		[Command("inviteuses")]
		[Summary("Invites a user to the server.")]
		[RequireUserPermission(GuildPermission.CreateInstantInvite)]
		public async Task InviteUserUses(string username, string descriminator, int maxInvites)
			=> await InviteUser(username, descriminator, null, maxInvites);

		[Name("Invite [Username] [UserDescriminator] [Days Active] [Max Invite]")]
		[Command("invite")]
		[Summary("Invites a user to the server.")]
		[RequireUserPermission(GuildPermission.CreateInstantInvite)]
		public async Task InviteUser(string username, string descriminator, int? days, int? maxInvites)
		{
			var channel = Context.Guild.SystemChannel;

			var invite = await channel.CreateInviteAsync(days, maxInvites);

			var user = Context.Client.GetUser(username, descriminator);
			if (user == null)
			{
				await ReplyAsync($"I haven't found anybody with username -> {username}");
				await invite.DeleteAsync();
			}
			await user.SendMessageAsync($"I hereby invite you to {invite.GuildName}, Come with me friend! https://discord.gg/{invite.Code}");
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