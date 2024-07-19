using MoviesTelegramBotApp.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

internal class UpdateHandler
{
    private readonly IBotService _botService;
    private readonly IMovieService _movieService;
    private int _moviePage = 1;

    public UpdateHandler(IBotService botService, IMovieService movieService)
    {
        _botService = botService;
        _movieService = movieService;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message && update.Message!.Type == MessageType.Text)
        {
            var chatId = update.Message.Chat.Id;
            var messageText = update.Message.Text;

            var startKeyBoard = new ReplyKeyboardMarkup(new KeyboardButton("Start")) { ResizeKeyboard = true };

            if (messageText?.ToLower() == "/start")
            {
                var botDetails = await _botService.GetBotDetailsAsync();
                var response = $"<strong>Lights, Camera, Action!</strong>\nWelcome to <em><strong>{botDetails.FirstName}</strong></em>, the hottest Telegram hangout for movie lovers! Whether you're a die-hard cinephile or just enjoy a good popcorn flick, this is your spot to discuss all things film.";
                await _botService.SendTextMessageAsync(chatId, response, ParseMode.Html);
                await SendMenuAsync(chatId, cancellationToken);
            }
            else
            {
                await HandleMenuResponseAsync(chatId, messageText!, cancellationToken);
            }
        }
    }

    private async Task SendMenuAsync(long chatId, CancellationToken cts)
    {
        var replyKeyBoardMarkup = new ReplyKeyboardMarkup(new[]
        {

            new KeyboardButton[] { "Movies", "Cartoons"}

        })
        {
            ResizeKeyboard = true
        };

        await _botService.SendTextMessageAsync(
            chatId,
            "Choose an option",
            parseMode: ParseMode.Html,
            replyKeyBoardMarkup,
            cancellationToken: cts);
    }

    private async Task MoviesMenu(long chatId, CancellationToken cts)
    {
        var replyKeyBoardMarkup = new ReplyKeyboardMarkup(new[]
        {

            new KeyboardButton[] { "Previous", "Next"}
            //new KeyboardButton[] { "Main Menu"}

        })
        {
            ResizeKeyboard = true
        };

        await _botService.SendTextMessageAsync(
            chatId,
            "Click Previous or Next",
            parseMode: ParseMode.Html,
            replyKeyBoardMarkup,
            cancellationToken: cts);
    }

    private async Task HandleMenuResponseAsync(long chatId, string messageText, CancellationToken cancellationToken)
    {
        switch (messageText)
        {
            case "Movies":
                await SendMoviesAsync(chatId, cancellationToken);
                await MoviesMenu(chatId, cancellationToken);

                break;
            case "Cartoons":
                //todo: functionality in progress
                break;

            case "Previous":
                //todo: functionality in progress
                break;

            case "Next":
                IncrementPage();
                await SendMoviesAsync(chatId, cancellationToken);
                await MoviesMenu(chatId, cancellationToken);
                break;

            default:
                await _botService.SendTextMessageAsync(chatId, "The command is not recognized");
                break;
        }
    }

    private async Task SendMoviesAsync(long chatId, CancellationToken cancellationToken)
    {
        var movies = await _movieService.GetAllMoviesAsync(_moviePage);
        var response = _movieService.BuildMoviesResponse(movies);

        foreach (var movie in movies)
        {
            if (!string.IsNullOrEmpty(movie.ImageUrl))
            {
                await _botService.SendPhotoWithInlineButtonUrlAsync(
                    chatId,
                    photoUrl: new Telegram.Bot.Types.InputFiles.InputOnlineFile(movie.ImageUrl),
                    caption: $"<strong>Title:</strong> {movie.Title}\n" +
                    $"<strong>Genre:</strong> {movie.Genre.Name}\n" +
                    $"<strong>Description:</strong> {movie.Description}\n" +
                    $"<strong>Country:</strong> {movie.Country}\n" +
                    $"<strong>Budget:</strong> {movie.Budget}\n" +
                    $"<strong>Interest facts:</strong> {movie.InterestFactsUrl}\n" +
                    $"<strong>Behind the scene:</strong> {movie.BehindTheScene}\n",
                    parseMode: ParseMode.Html,
                    replyMarkup: new InlineKeyboardMarkup(
            InlineKeyboardButton.WithUrl("Check out the trailer", movie.MovieUrl)));
            }
            else
            {
                await _botService.SendTextMessageAsync(chatId, response, cancellationToken);
            }
        }
    }

    private void IncrementPage() => ++_moviePage;

    private void DecrementPage() => --_moviePage;
}