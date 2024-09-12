using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoviesTelegramBotApp.Interfaces;
using MoviesTelegramBotApp.Services;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace MoviesTelegramBotApp
{
    public class Program
    {
        private static IHost? _host;

        public static void Main()
        {
            _host = CreateHostBuilder().Build();

            var botClient = _host.Services.GetRequiredService<IBotService>().Client;
            var updateHandler = _host.Services.GetRequiredService<IUpdateHandler>();

            using var cts = new CancellationTokenSource();

            botClient.StartReceiving(
                async (bot, update, token) => await updateHandler.HandleUpdateAsync(bot, update, token),
                async (bot, exception, token) => await updateHandler.HandlePollingErrorAsync(bot, exception, token),
                receiverOptions: new ReceiverOptions(),
                cancellationToken: cts.Token
                );

            var logger = _host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Bot is up and running");

            Console.ReadLine();
            cts.Cancel();
        }

        public static IHostBuilder CreateHostBuilder() =>
        Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection")));

                // register the bit with API key
                services.AddSingleton<IBotService>(sp =>
                new BotService(context.Configuration["TelegramBot:ApiKey"]));

                // register the UpdateHandler first
                services.AddTransient<UpdateHandler>();

                // register the ExceptionHandlingMiddleware, passing the UpdateHandler as the next handler
                services.AddTransient<IUpdateHandler>(sp =>
                new ExceptionHandlingMiddleware(
                    sp.GetRequiredService<UpdateHandler>(),
                    sp.GetRequiredService<ILogger<ExceptionHandlingMiddleware>>(),
                    sp.GetRequiredService<IBotService>().Client,
                    sp.GetRequiredService<IConfiguration>()
                    ));

                services.AddTransient<IMovieService, MovieService>();
                services.AddTransient<Random>();
                services.AddTransient<ICartoonService, CartoonService>();

                services.AddLogging(configure => configure.AddConsole());
            })
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                config.AddEnvironmentVariables();
            });
    }
}