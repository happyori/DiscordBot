using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
	[Name("Help")]
	public class HelpModule : ModuleBase<SocketCommandContext>
	{
		private readonly CommandService _services;
		private readonly IConfigurationRoot _config;

		public HelpModule(CommandService services, IConfigurationRoot config)
		{
			_services = services;
			_config = config;
		}

		[Name("Help")]
		[Command("help")]
		public async Task HelpAsync()
		{
			string prefix = _config["prefix"];
			var authorbuilder = new EmbedAuthorBuilder()
			{
				Name = Globals.Msg.Author.Username,
				IconUrl = Globals.Msg.Author.GetAvatarUrl(),
			};

			var footerbuilder = new EmbedFooterBuilder()
			{
				Text = "Powered by your's truly"
			};

			var builder = new EmbedBuilder()
			{
				Color = new Color(86, 20, 127),
				Description = "These are the commands you can use",
				Author = authorbuilder,
				Footer = footerbuilder,
			};

			foreach (var module in _services.Modules)
			{
				string description = null;

				foreach (var cmd in module.Commands)
				{
					var result = await cmd.CheckPreconditionsAsync(Context);
					if (result.IsSuccess && (!module.IsSubmodule || string.IsNullOrWhiteSpace(module.Parent.Aliases.First())))
						description += $"{cmd.Name} : {prefix}{cmd.Aliases.First()}\n";
					else if (result.IsSuccess && module.IsSubmodule)
						description += $"{cmd.Name} : {prefix}{module.Parent.Aliases.First()} {cmd.Aliases.First()}\n";
				}

				if (!string.IsNullOrWhiteSpace(description))
				{
					builder.AddField(x =>
					{
						x.Name = module.Name;
						x.Value = description;
						x.IsInline = false;
					});
				}
			}

			await ReplyAsync("", false, builder.Build());
		}

		[Name("Help [Command]")]
		[Command("help")]
		public async Task HelpAsync([Remainder] string commandName)
		{
			var result = _services.Search(Context, commandName);

			if (!result.IsSuccess)
			{
				await ReplyAsync($"Sorry, I couldn't find the command : **{commandName}**");
				return;
			}

			string prefix = _config["prefix"];

			var authorbuilder = new EmbedAuthorBuilder()
			{
				Name = Globals.Msg.Author.Username,
				IconUrl = Globals.Msg.Author.GetAvatarUrl(),
			};

			var footerbuilder = new EmbedFooterBuilder()
			{
				Text = "Powered by your's truly"
			};

			var builder = new EmbedBuilder()
			{
				Color = new Color(86, 20, 127),
				Description = $"Here are some commands like **{commandName}**",
				Author = authorbuilder,
				Footer = footerbuilder,
			};

			foreach (var match in result.Commands)
			{
				var cmd = match.Command;

				builder.AddField(x =>
				{
					x.Name = string.Join(", ", cmd.Aliases);
					x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" +
							  $"Summary: {cmd.Summary}";
					x.IsInline = false;
				});
			}

			await ReplyAsync("", false, builder.Build());
		}
	}
}