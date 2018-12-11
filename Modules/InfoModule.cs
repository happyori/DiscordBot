using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
	[Name("Info")]
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
		[Name("Calc [Number] [Sign] [Number]")]
		[Command("calc"), Alias("c")]
		[Summary("Calculates an answer for you!")]
		public async Task Calc([Remainder] string command)
		{
			char[] signs = new char[] {'+', '-', '*', '/', '^', '%'};
			char sign = '\0';
			double num1 = 0;
			double num2 = 0;
			double result = 0;

			command = command.ToLower();
			command = command.Trim();

			var authorbuilder = new EmbedAuthorBuilder()
			{
				Name = Globals.msg.Author.Username,
				IconUrl = Globals.msg.Author.GetAvatarUrl(),
			};

			var footerbuilder = new EmbedFooterBuilder()
			{
				Text = "Powered by your's truly"
			};

			var builder = new EmbedBuilder()
			{
				Color = new Color(255, 0, 135),
				Description = "Sans de Calculato",
				Author = authorbuilder,
				Footer = footerbuilder,
			};

			if (!command.Contains('+') &&
				!command.Contains('-') &&
				!command.Contains('*') &&
				!command.Contains('/') &&
				!command.Contains('^') &&
				!command.Contains('%'))
			{
				await UsageCalc();
				return ;
			}

			sign = command[command.IndexOfAny(signs)];

			string[] split = command.Split(sign, 2, StringSplitOptions.RemoveEmptyEntries);

			if (!double.TryParse(split[0], out num1))
			{
				await UsageCalc();
				return ;
			}

			if (!double.TryParse(split[1], out num2))
			{
				await UsageCalc();
				return ;
			}

			if (sign == '/' && num2 == 0)
			{
				await ReplyAsync($"It is truly unholy to divide by 0\n{Globals.msg.Author.Mention} should be ashamed of yourself!");
				return ;
			}

			switch (sign)
			{
				case '-':
					result = num1 - num2;
					break;
				case '*':
					result = num1 * num2;
					break;
				case '/':
					result = num1 / num2;
					break;
				case '%':
					result = num1 % num2;
					break;
				case '^':
					result = Math.Pow(num1, num2);
					break;
				default:
					result = num1 + num2;
					break;
			}

			builder.AddField(x =>
			{
				x.Name = "Calculate:";
				x.Value = $" {num1} {sign} {num2} ";
			});
			builder.AddField(x =>
			{
				x.Name = "__RESULTS__";
				x.Value = $"**{result}**";
			});

			await ReplyAsync("", false, builder.Build());
		}

		private async Task UsageCalc()
		{
			var authorbuilder = new EmbedAuthorBuilder()
			{
				Name = Globals.msg.Author.Username,
				IconUrl = Globals.msg.Author.GetAvatarUrl(),
			};

			var footerbuilder = new EmbedFooterBuilder()
			{
				Text = "Powered by your's truly"
			};

			var builder = new EmbedBuilder()
			{
				Color = new Color(38, 196, 255),
				Author = authorbuilder,
				Footer = footerbuilder,
			};
			builder.AddField(x =>
			{
				x.Name = "Usage:";
				x.Value = "!calc(!c) Number (+ - / * ^ %) Number";
			});
			builder.AddField(x =>
			{
				x.Name = "Example:";
				x.Value = "!c 20 + 20";
				x.IsInline = false;
			});
			builder.AddField(x =>
			{
				x.Name = "**RESULT**";
				x.Value = "**40**";
			});
			await ReplyAsync("", false, builder.Build());
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