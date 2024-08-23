using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace MoviesTelegramBotApp.Interfaces
{
    public interface IBotService
    {
        ITelegramBotClient Client { get; }
        Task SendTextMessageAsync(
            long chatId, string response);
        Task SendTextMessageAsync(
            long chatId,
            string response,
            CancellationToken cancellationToken);
        Task SendTextMessageAsync(
            long chatId,
            string response,
            ParseMode parseMode);
        Task SendTextMessageAsync(
            long chatId,
            string message,
            ParseMode parseMode,
            ReplyKeyboardMarkup markup,
            CancellationToken cancellationToken);
        Task SendPhotoWithInlineButtonUrlAsync(
            long chatId,
            InputOnlineFile photoUrl,
            string caption,
            ParseMode parseMode,
            InlineKeyboardMarkup replyMarkup,
            CancellationToken cancellation);
        Task<User> GetBotDetailsAsync();
    }
}
