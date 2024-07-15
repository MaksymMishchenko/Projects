using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoviesTelegramBotApp.Interfaces;
using MoviesTelegramBotApp.Services;
using Telegram.Bot;

namespace MoviesTelegramBotApp
{
    internal static class Startup
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection")));

                //var apiKey = context.Configuration["TelegramBot:ApiKey"];
                services.AddSingleton(new TelegramBotClient("7480245378:AAGTHC66vyBSIZOSU4M68n1QEiFb-b_-5rk"));

                services.AddSingleton<IBotService, BotService>();
                services.AddSingleton<IMovieService, MovieService>();
                services.AddSingleton<UpdateHandler>();
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
