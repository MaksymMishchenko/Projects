using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class ExceptionHandlingMiddleware : IUpdateHandler
{
    private readonly IUpdateHandler _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    ITelegramBotClient _botClient;
    private readonly long _adminChatId;

    public ExceptionHandlingMiddleware(
        IUpdateHandler next,
        ILogger<ExceptionHandlingMiddleware> logger,
        ITelegramBotClient botService,
        IConfiguration configuration
        )
    {
        _next = next;
        _logger = logger;
        _botClient = botService;
        _adminChatId = long.Parse(configuration["AdminChatId"]); // Retrieve admin chat ID from configuration
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            // Pass the update to the next handler
            await _next.HandleUpdateAsync(botClient, update, cancellationToken);
        }
        catch (Exception ex)
        {
            // Log the exception
            _logger.LogInformation("MY global exception works!!!!");
            _logger.LogError(ex, "An unhandled exception occurred while processing an update.");

            // Notify the admin about the exception
            await NotifyAdminAsync(ex);

            // Optionally, inform the user about the error
            if (update.Type == UpdateType.Message && update.Message != null)
            {
                await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: "An error occurred while processing your request. Please try again later.",
                    cancellationToken: cancellationToken);
            }
        }
    }

    private async Task NotifyAdminAsync(Exception ex)
    {
        string errorMessage = $"Hey Admin! Exception occurred: {ex.Message}\n\n{ex.StackTrace}";
        await _botClient.SendTextMessageAsync(_adminChatId, errorMessage);
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred while polling.");

        return Task.CompletedTask;
    }
}
