using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _services;
        private readonly IConfigurationRoot _config;

		public HelpModule(CommandService services, IConfigurationRoot config)
		{
			_services = services;
			_config = config;
		}

		[Command("help")]
		public async Task HelpAsync()
		{
			string prefix = _config["prefix"];
			var builder = new EmbedBuilder()
			{
				Color = new Color(114, 137, 210),
				Description = "These are the commands you use"
			};

			foreach (var module in _services.Modules)
			{
				string description = null;

				foreach (var cmd in module.Commands)
				{
					var result = await cmd.CheckPreconditionsAsync(Context);
					if (result.IsSuccess)
						description += $"{prefix}{cmd.Aliases.First()}\n";
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

		[Command("help")]
		public async Task HelpAsync(string commandName)
		{
			var result = _services.Search(Context, commandName);

			if (!result.IsSuccess)
			{
				await ReplyAsync($"Sorry, I couldn't find the command : **{commandName}**");
				return ;
			}

			string prefix = _config["prefix"];
			var builder = new EmbedBuilder()
			{
				Color = new Color(114, 137, 210),
				Description = $"Here are some commands like **{commandName}**"
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