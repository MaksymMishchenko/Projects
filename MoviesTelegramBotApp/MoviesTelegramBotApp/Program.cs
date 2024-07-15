using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoviesTelegramBotApp.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace MoviesTelegramBotApp
{
    internal class Program
    {
        private static IHost _host;
        private static CancellationTokenSource _cts;       

        public static async Task Main(string[] args)
        {      
            _host = Startup.CreateHostBuilder(args).Build();
            _cts = new CancellationTokenSource();

            var botService = _host.Services.GetRequiredService<IBotService>();
            var updateHandler = _host.Services.GetRequiredService<UpdateHandler>();

            var recieverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            var botClient = _host.Services.GetRequiredService<TelegramBotClient>();
            botClient.StartReceiving(updateHandler.HandleUpdateAsync, HandleErrorAsync, recieverOptions, _cts.Token);

            var logger = _host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Bot is up and running");

            Console.ReadLine();
            _cts.Cancel();

            await Task.Delay(1000);
        }

        private static Task HandleErrorAsync(ITelegramBotClient bot, Exception ex, CancellationToken cts)
        {
            var logger = _host.Services.GetRequiredService<ILogger<Program>>();

            var errorMessage = ex switch
            {
                ApiRequestException apiRequestException =>
                $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}"
            };

            logger.LogError(errorMessage);
            return Task.CompletedTask;
        }
    }
}