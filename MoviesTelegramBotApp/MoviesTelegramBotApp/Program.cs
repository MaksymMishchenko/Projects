using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MoviesTelegramBotApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var host = Startup.CreateHostBuilder(args).Build();

            // Example of using the DbContext
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ApplicationDbContext>();

                // Use the context here
                context.Database.EnsureCreated();                
            }
        }        
    }
}