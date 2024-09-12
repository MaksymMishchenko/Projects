using Microsoft.Data.SqlClient;
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
        ITelegramBotClient botClient,
        IConfiguration configuration
        )
    {
        _next = next;
        _logger = logger;
        _botClient = botClient;
        _adminChatId = long.Parse(configuration["AdminChatId"]);
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Handling update...");
            await _next.HandleUpdateAsync(botClient, update, cancellationToken);
        }
        catch (SqlException ex)
        {
            _logger.LogCritical(ex, "A database error occurred during update handling.");
            await NotifyAdminAsync(
                "Hey admin! Critical error: couldn't connect to the database. Immediate attention required.",
                cancellationToken);

            if (update.Message?.Chat != null)
            {
                await _botClient.SendTextMessageAsync(update.Message.Chat.Id,
                    "We are experiencing database issues. Please try again later.",
                    cancellationToken: cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while processing an update.");

            await NotifyAdminAsync(ex, cancellationToken);

            if (update.Type == UpdateType.Message && update.Message != null)
            {
                await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: "An error occurred while processing your request. Please try again later.",
                    cancellationToken: cancellationToken);
            }
        }
    }

    private async Task NotifyAdminAsync(Exception ex, CancellationToken cancellationToken)
    {
        var adminMessage = $"Hey Admin! Exception occurred: {ex.Message}\n\n{ex.StackTrace}";
        await _botClient.SendTextMessageAsync(_adminChatId, adminMessage, cancellationToken: cancellationToken);
    }

    private async Task NotifyAdminAsync(string message, CancellationToken cancellationToken)
    {
        await _botClient.SendTextMessageAsync(_adminChatId, message, cancellationToken: cancellationToken);
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred while polling.");

        return Task.CompletedTask;
    }
}
