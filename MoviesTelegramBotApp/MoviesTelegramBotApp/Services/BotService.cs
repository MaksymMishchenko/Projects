using MoviesTelegramBotApp.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace MoviesTelegramBotApp.Services
{
    internal class BotService : IBotService
    {
        public ITelegramBotClient Client { get; }

        public BotService(string apiKey)
        {
            Client = new TelegramBotClient(apiKey);
        }
        public async Task<User> GetBotDetailsAsync()
        {
            return await Client.GetMeAsync();
        }

        public async Task SendTextMessageAsync(
            long chatId,
            string message,
            ParseMode parseMode,
            ReplyKeyboardMarkup replyMarkup,
            CancellationToken cancellationToken)
        {
            await Client.SendTextMessageAsync(
                chatId,
                message,
                parseMode,
                replyMarkup: replyMarkup,
                cancellationToken: cancellationToken);
        }

        public async Task SendTextMessageAsync(long chatId, string response, ParseMode parseMode)
        {
            await Client.SendTextMessageAsync(
                chatId,
                response,
                parseMode);
        }

        public async Task SendTextMessageAsync(long chatId, string response)
        {
            await Client.SendTextMessageAsync(chatId, response);
        }

        public async Task SendTextMessageAsync(long chatId, string response, CancellationToken cancellationToken)
        {
            await Client.SendTextMessageAsync(chatId, response, cancellationToken: cancellationToken);
        }

        public async Task SendPhotoWithInlineButtonUrlAsync(
            long chatId,
            InputOnlineFile photoUrl,
            string caption,
            ParseMode parseMode,
            InlineKeyboardMarkup replyMarkup,
            CancellationToken cancellationToken)
        {
            await Client.SendPhotoAsync(
                chatId: chatId,
                photo: photoUrl,
                caption: caption,
                parseMode: parseMode,
                replyMarkup: replyMarkup,
                cancellationToken: cancellationToken);
        }
    }
}
