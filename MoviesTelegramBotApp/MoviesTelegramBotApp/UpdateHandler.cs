using MoviesTelegramBotApp.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

internal class UpdateHandler
{
    private readonly IBotService _botService;
    private readonly IMovieService _movieService;
    private readonly ICartoonService _cartoonService;
    private int _moviePage = 1;
    private int _cartoonPage = 1;

    public UpdateHandler(IBotService botService, IMovieService movieService, ICartoonService cartoonService)
    {
        _botService = botService;
        _movieService = movieService;
        _cartoonService = cartoonService;
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
                // Movies, Cartoons menu
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
        var replyKeyBoardMarkup = new ReplyKeyboardMarkup(new[] { new KeyboardButton[] { "Movies", "Cartoons" } }) { ResizeKeyboard = true };

        await _botService.SendTextMessageAsync(
            chatId,
            "Choose an option:",
            parseMode: ParseMode.Html,
            replyKeyBoardMarkup,
            cancellationToken: cts);
    }

    private async Task SendMoviesNavAsync(long chatId, CancellationToken cts)
    {
        var totalMovies = await _movieService.Count;

        bool showPrevious = _moviePage > 1;
        bool showNext = _moviePage < totalMovies;

        var buttons = new List<KeyboardButton[]>();

        if (showPrevious && showNext)
        {
            buttons.Add(new KeyboardButton[] { "Prev Movie", "Next Movie" });
            buttons.Add(new KeyboardButton[] { "Go Back" });
        }
        else if (showPrevious)
        {
            buttons.Add(new KeyboardButton[] { "Prev Movie" });
            buttons.Add(new KeyboardButton[] { "Go Back" });
        }
        else if (showNext)
        {
            buttons.Add(new KeyboardButton[] { "Next Movie" });
            buttons.Add(new KeyboardButton[] { "Go Back" });
        }

        var replyKeyBoardMarkup = new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };

        await _botService.SendTextMessageAsync(
            chatId,
            "Choose an option:",
            parseMode: ParseMode.Html,
            replyKeyBoardMarkup,
            cancellationToken: cts);
    }

    private async Task SendCartoonsNavAsync(long chatId, CancellationToken cts)
    {
        var totalCartoons = await _cartoonService.Count;

        bool showPrevious = _cartoonPage > 1;
        bool showNext = _cartoonPage < totalCartoons;

        var buttons = new List<KeyboardButton[]>();

        if (showPrevious && showNext)
        {
            buttons.Add(new KeyboardButton[] { "Prev Cartoon", "Next Cartoon" });
            buttons.Add(new KeyboardButton[] { "Go Back" });
        }
        else if (showPrevious)
        {
            buttons.Add(new KeyboardButton[] { "Prev Cartoon" });
            buttons.Add(new KeyboardButton[] { "Go Back" });
        }
        else if (showNext)
        {
            buttons.Add(new KeyboardButton[] { "Next Cartoon" });
            buttons.Add(new KeyboardButton[] { "Go Back" });
        }

        var replyKeyBoardMarkup = new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };

        await _botService.SendTextMessageAsync(
            chatId,
            "Choose an option:",
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
                await SendMoviesNavAsync(chatId, cancellationToken);
                break;

            case "Cartoons":
                await SendCartoonsAsync(chatId, cancellationToken);
                await SendCartoonsNavAsync(chatId, cancellationToken);
                break;

            case "Next Movie":
                IncrementMoviePage();
                await SendMoviesAsync(chatId, cancellationToken);
                await SendMoviesNavAsync(chatId, cancellationToken);
                break;

            case "Prev Movie":
                DecrementMoviePage();
                await SendMoviesAsync(chatId, cancellationToken);
                await SendMoviesNavAsync(chatId, cancellationToken);
                break;

            case "Next Cartoon":
                IncrementCartoonPage();
                await SendCartoonsAsync(chatId, cancellationToken);
                await SendCartoonsNavAsync(chatId, cancellationToken);
                break;

            case "Prev Cartoon":
                DecrementCartoonPage();
                await SendCartoonsAsync(chatId, cancellationToken);
                await SendCartoonsNavAsync(chatId, cancellationToken);
                break;

            case "Go Back":
                await SendMenuAsync(chatId, cancellationToken);
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
                    $"<strong>Genre:</strong> {movie?.Genre?.Name}\n" +
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

    private async Task SendCartoonsAsync(long chatId, CancellationToken cancellationToken)
    {
        var cartoons = await _cartoonService.GetAllCartoonsAsync(_cartoonPage);

        foreach (var cartoon in cartoons)
        {
            await _botService.SendPhotoWithInlineButtonUrlAsync(
                chatId,
                photoUrl: new Telegram.Bot.Types.InputFiles.InputOnlineFile(cartoon.ImageUrl),
                caption: $"<strong>Title:</strong> {cartoon.Title}\n" +
                $"<strong>Genre:</strong> {cartoon?.Genre?.Genre}\n" +
                $"<strong>Description:</strong> {cartoon?.Description}\n" +
                $"<strong>Budget:</strong> {cartoon?.Budget}\n",
                parseMode: ParseMode.Html,
                replyMarkup: new InlineKeyboardMarkup(
        InlineKeyboardButton.WithUrl("Check out the cartoon", cartoon.CartoonUrl)));
        }
    }

    private void IncrementMoviePage() => ++_moviePage;

    private void DecrementMoviePage() => --_moviePage;

    private void IncrementCartoonPage() => ++_cartoonPage;

    private void DecrementCartoonPage() => --_cartoonPage;
}