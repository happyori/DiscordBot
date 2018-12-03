using System.Threading.Tasks;

namespace DiscordBot
{
    public class Program
    {
        public static Task Main(string[] args)
			=> Startup.RunAsync(args);
    }
}