using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot
{
    class Program
    {
        private DiscordSocketClient _client;
        private InteractionService _commands;
        private IServiceProvider _services;
        private const string Token = ""; // Replace with your bot's token
        private const ulong GuildId = 9; // Replace with your guild ID

        static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        public async Task RunBotAsync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            });

            _commands = new InteractionService(_client.Rest);

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            _client.Log += Log;
            _client.Ready += OnReadyAsync;
            _client.InteractionCreated += HandleInteraction;

            await _client.LoginAsync(TokenType.Bot, Token);
            await _client.StartAsync();

            await _commands.AddModulesAsync(System.Reflection.Assembly.GetEntryAssembly(), _services);

            // Block the program until it is closed.
            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task OnReadyAsync()
        {
            // Register commands to a specific guild
            await _commands.RegisterCommandsToGuildAsync(GuildId);
            Console.WriteLine($"{_client.CurrentUser} is connected and ready!");
        }

        private async Task HandleInteraction(SocketInteraction interaction)
        {
            try
            {
                var context = new SocketInteractionContext(_client, interaction);
                await _commands.ExecuteCommandAsync(context, _services);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling interaction: {ex}");
            }
        }
    }

    public class SlashCommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("hello", "Says hello")]
        public async Task Hello()
        {
            await RespondAsync("You da best hu hu!");
        }
    }
}

