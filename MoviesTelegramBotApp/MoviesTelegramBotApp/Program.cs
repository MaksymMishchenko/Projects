using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoviesTelegramBotApp.Interfaces;
using MoviesTelegramBotApp.Services;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace MoviesTelegramBotApp
{
    internal class Program
    {
        private static CancellationTokenSource _cts;
        private static IHost _host;

        public static void Main()
        {
            _host = CreateHostBuilder().Build();

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
        }

        public static IHostBuilder CreateHostBuilder() =>
        Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection")));

                var apiKey = context.Configuration["TelegramBot:ApiKey"];
                services.AddSingleton(new TelegramBotClient(apiKey));

                services.AddTransient<IMovieService, MovieService>();
                services.AddSingleton<IBotService, BotService>();
                //services.AddTransient<IMovieService, MovieService>();
                services.AddScoped<UpdateHandler>();
                services.AddLogging(configure => configure.AddConsole());
            })
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                config.AddEnvironmentVariables();
            });

        private static Task HandleErrorAsync(ITelegramBotClient bot, Exception ex, CancellationToken cts)
        {
            var logger = _host.Services.GetRequiredService<ILogger<Program>>();

            var errorMessage = ex switch
            {
                ApiRequestException apiRequestException =>
                $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}"
            };

            //logger.LogError(errorMessage);
            return Task.CompletedTask;
        }
    }
}