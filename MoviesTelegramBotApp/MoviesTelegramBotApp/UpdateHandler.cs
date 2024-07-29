using Microsoft.Extensions.Logging;
using MoviesTelegramBotApp.Interfaces;
using MoviesTelegramBotApp.Models;
using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

internal class UpdateHandler
{
    private readonly IBotService _botService;
    private readonly IMovieService _movieService;
    private readonly ICartoonService _cartoonService;
    private readonly ConcurrentDictionary<long, (string state, string searchString)> _userStates;
    private readonly ILogger<UpdateHandler> _logger;
    private int _moviePage = 1;
    private int _moviePageByTitle = 1;
    private int _cartoonPage = 1;

    private const string StateAwaitingMovieSearch = "awaiting_movie_search";
    private const string StateNavigatingMovies = "navigating_movies";

    public UpdateHandler(
        IBotService botService,
        IMovieService movieService,
        ICartoonService cartoonService,
        ILogger<UpdateHandler> logger)
    {
        _botService = botService;
        _movieService = movieService;
        _cartoonService = cartoonService;
        _userStates = new ConcurrentDictionary<long, (string state, string searchString)>();
        _logger = logger;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message && update.Message!.Type == MessageType.Text)
        {
            var chatId = update.Message.Chat.Id;
            var messageText = update.Message.Text;

            if (_userStates.TryGetValue(chatId, out var userState))
            {
                if (userState.state == StateAwaitingMovieSearch)
                {
                    if (messageText == "🔎 Search")
                    {
                        await _botService.SendTextMessageAsync(chatId, "Please, enter a title of movie", cancellationToken);

                        return;
                    }
                    await GetFoundMoviesAsync(messageText!, chatId, cancellationToken);
                    _userStates[chatId] = (StateNavigatingMovies, messageText!);
                }
                else if (userState.state == StateNavigatingMovies)
                {
                    if (messageText == "Next ⏭️")
                    {
                        IncrementMoviePageByTitle();
                        await GetFoundMoviesAsync(userState.searchString, chatId, cancellationToken);
                    }
                    else if (messageText == "⏮️ Prev")
                    {
                        DecrementMoviePageByTitle();
                        await GetFoundMoviesAsync(userState.searchString, chatId, cancellationToken);
                    }
                    else if (messageText == "Main Menu 🔝")
                    {
                        await SendMenuAsync(chatId, cancellationToken);
                    }
                    else
                    {
                        await HandleMenuResponseAsync(chatId, messageText, cancellationToken);
                    }
                }
            }
            else
            {
                if (messageText?.ToLower() == "/start")
                {
                    var botDetails = await _botService.GetBotDetailsAsync();
                    var response = $"<strong>Lights, Camera, Action! 🎥</strong>\nWelcome to <em><strong>{botDetails.FirstName} 😀</strong></em>, the hottest Telegram hangout for movie lovers!🔥 Whether you're a die-hard cinephile or just enjoy a good popcorn flick 🍿, this is your spot to discuss all things film 📽️.";
                    await _botService.SendTextMessageAsync(chatId, response, ParseMode.Html);
                    await SendMenuAsync(chatId, cancellationToken);
                }
                else
                {
                    await HandleMenuResponseAsync(chatId, messageText!, cancellationToken);
                }
            }
        }
    }

    private async Task SendMenuAsync(long chatId, CancellationToken cts)
    {
        var replyKeyBoardMarkup = new ReplyKeyboardMarkup(new[] { new KeyboardButton[] { "🎥 Movies", "🎞️ Cartoons", "✨ Surprise Me", "🔎 Search" } }) { ResizeKeyboard = true };

        await _botService.SendTextMessageAsync(
            chatId,
            "Choose an option, please: 🔽",
            parseMode: ParseMode.Html,
            replyKeyBoardMarkup,
            cancellationToken: cts);
    }

    private async Task SendMoviesNavAsync(long chatId, CancellationToken cts)
    {
        var totalMovies = await _movieService.CountAsync;
        bool showPrevious = _moviePage > 1;
        bool showNext = _moviePage < totalMovies;

        await SendNavigationAsync(chatId, cts, showPrevious, showNext, "Main Menu 🔝", "⏮️ Prev Movie", "Next Movie ⏭️");
    }

    private async Task SendMoviesByTitleNavAsync(string searchString, long chatId, CancellationToken cts)
    {
        var totalMovies = await _movieService.GetMoviesByTitleCountAsync(searchString);
        bool showPrevious = _moviePageByTitle > 1;
        bool showNext = _moviePageByTitle < totalMovies;

        await SendNavigationAsync(chatId, cts, showPrevious, showNext, "Main Menu 🔝", "⏮️ Prev", "Next ⏭️");
    }

    private async Task SendCartoonsNavAsync(long chatId, CancellationToken cts)
    {
        var totalCartoons = await _cartoonService.Count;
        bool showPrevious = _cartoonPage > 1;
        bool showNext = _cartoonPage < totalCartoons;

        await SendNavigationAsync(chatId, cts, showPrevious, showNext, "Main Menu 🔝", "⏮️ Prev Cartoon", "Next Cartoon ⏭️");
    }

    private async Task SendNavigationAsync(long chatId,
        CancellationToken cts,
        bool showPrevious,
        bool showNext, string backButtonText,
        string previousButtonText,
        string nextButtonText)
    {
        var buttons = new List<KeyboardButton[]>();

        if (showPrevious && showNext)
        {
            buttons.Add(new KeyboardButton[] { previousButtonText, nextButtonText });
            buttons.Add(new KeyboardButton[] { backButtonText });
        }
        else if (showPrevious)
        {
            buttons.Add(new KeyboardButton[] { previousButtonText });
            buttons.Add(new KeyboardButton[] { backButtonText });
        }
        else if (showNext)
        {
            buttons.Add(new KeyboardButton[] { nextButtonText });
            buttons.Add(new KeyboardButton[] { backButtonText });
        }

        var replyKeyBoardMarkup = new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };

        await _botService.SendTextMessageAsync(
            chatId,
            "<b>Click ⏮️ Prev or Next ⏭️ to navigate</b>",
            parseMode: ParseMode.Html,
            replyKeyBoardMarkup,
            cancellationToken: cts);
    }

    private async Task HandleMenuResponseAsync(long chatId, string messageText, CancellationToken cancellationToken)
    {
        switch (messageText)
        {
            case "🎥 Movies":
                await GetMoviesAsync(chatId, cancellationToken);
                await SendMoviesNavAsync(chatId, cancellationToken);
                break;

            case "🎞️ Cartoons":
                await SendCartoonsAsync(chatId, cancellationToken);
                await SendCartoonsNavAsync(chatId, cancellationToken);
                break;

            case "Next Movie ⏭️":
                IncrementMoviePage();
                await GetMoviesAsync(chatId, cancellationToken);
                await SendMoviesNavAsync(chatId, cancellationToken);
                break;

            case "⏮️ Prev Movie":
                DecrementMoviePage();
                await GetMoviesAsync(chatId, cancellationToken);
                await SendMoviesNavAsync(chatId, cancellationToken);
                break;

            case "✨ Surprise Me":
                await GetRandomMovieAsync(chatId, cancellationToken);
                break;

            case "Next Cartoon ⏭️":
                IncrementCartoonPage();
                await SendCartoonsAsync(chatId, cancellationToken);
                await SendCartoonsNavAsync(chatId, cancellationToken);
                break;

            case "⏮️ Prev Cartoon":
                DecrementCartoonPage();
                await SendCartoonsAsync(chatId, cancellationToken);
                await SendCartoonsNavAsync(chatId, cancellationToken);
                break;

            case "🔎 Search":
                await _botService.SendTextMessageAsync(chatId, "🔍 Please enter a movie you want to find\n  For example: 'The Mask'", ParseMode.Html);
                _userStates[chatId] = (StateAwaitingMovieSearch, string.Empty);
                break;

            case "Main Menu 🔝":
                await SendMenuAsync(chatId, cancellationToken);
                break;
        }
    }

    private async Task GetMoviesAsync(long chatId, CancellationToken cancellationToken)
    {
        var tasks = new List<Task>();

        try
        {
            var getAllMoviesAsync = await _movieService.GetAllMoviesAsync(_moviePage);
            var sendMoviesAsync = SendMoviesAsync(getAllMoviesAsync, chatId, cancellationToken);
            tasks.Add(sendMoviesAsync);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex.Message);
            var exResponse = "We are sorry. 😟\nService is unavailable. We do our best to resolve this mistake.";
            var sendTextMessageAsync = _botService.SendTextMessageAsync(chatId, exResponse, cancellationToken: cancellationToken);
            tasks.Add(sendTextMessageAsync);
        }

        await Task.WhenAll(tasks);
    }

    private async Task GetRandomMovieAsync(long chatId, CancellationToken cts)
    {
        var tasks = new List<Task>();

        try
        {
            var rndMovie = await _movieService.GetRandomMovieAsync();
            await SendMoviesAsync(rndMovie, chatId, cts);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex.Message);
            var sendTextMessageAsync = _botService.SendTextMessageAsync(chatId, "Sorry, the movie is not available 😟");
            tasks.Add(sendTextMessageAsync);
        }
        finally
        {
            await Task.WhenAll(tasks);
        }
    }

    private async Task SendMoviesAsync(IEnumerable<Movie> movies, long chatId, CancellationToken cancellationToken)
    {
        var tasks = new List<Task>();

        foreach (var movie in movies)
        {
            var task = _botService.SendPhotoWithInlineButtonUrlAsync(
                 chatId,
                 photoUrl: new Telegram.Bot.Types.InputFiles.InputOnlineFile(movie.ImageUrl),
                 caption: $"<strong>▶️ Title:</strong> {movie.Title}\n" +
                 $"<strong>🎬 Genre:</strong> {movie.Genre.Name}\n" +
                 $"<strong>🧾 Description:</strong> {movie.Description}\n" +
                 $"<strong>🌍 Country:</strong> {movie.Country}\n" +
                 $"<strong>💸 Budget:</strong> {movie.Budget}\n" +
                 $"<strong>📌 Interest facts:</strong> {movie.InterestFactsUrl}\n" +
                 $"<strong>✨ Behind the scene:</strong> {movie.BehindTheScene}\n",
                 parseMode: ParseMode.Html,
                 replyMarkup: new InlineKeyboardMarkup(
         InlineKeyboardButton.WithUrl("Check out the trailer", movie.MovieUrl)),
                 cancellationToken);

            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
    }

    private async Task SendMoviesAsync(Movie movie, long chatId, CancellationToken cancellationToken)
    {
        var tasks = new List<Task>();

        var task = _botService.SendPhotoWithInlineButtonUrlAsync(
             chatId,
             photoUrl: new Telegram.Bot.Types.InputFiles.InputOnlineFile(movie.ImageUrl),
             caption: $"<strong>▶️ Title:</strong> {movie.Title}\n" +
             $"<strong>🎬 Genre:</strong> {movie.Genre.Name}\n" +
             $"<strong>🧾 Description:</strong> {movie.Description}\n" +
             $"<strong>🌍 Country:</strong> {movie.Country}\n" +
             $"<strong>💸 Budget:</strong> {movie.Budget}\n" +
             $"<strong>📌 Interest facts:</strong> {movie.InterestFactsUrl}\n" +
             $"<strong>✨ Behind the scene:</strong> {movie.BehindTheScene}\n",
             parseMode: ParseMode.Html,
             replyMarkup: new InlineKeyboardMarkup(
     InlineKeyboardButton.WithUrl("👉 Check out the trailer", movie.MovieUrl)),
             cancellationToken);

        tasks.Add(task);

        await Task.WhenAll(tasks);
    }

    private async Task SendCartoonsAsync(long chatId, CancellationToken cancellationToken)
    {
        var cartoons = await _cartoonService.GetAllCartoonsAsync(_cartoonPage);
        var tasks = new List<Task>();

        foreach (var cartoon in cartoons)
        {
            var task = _botService.SendPhotoWithInlineButtonUrlAsync(
                 chatId,
                 photoUrl: new Telegram.Bot.Types.InputFiles.InputOnlineFile(cartoon.ImageUrl),
                 caption: $"<strong>▶️ Title:</strong> {cartoon.Title}\n" +
                 $"<strong>🎬 Genre:</strong> {cartoon?.Genre?.Genre}\n" +
                 $"<strong>🧾 Description:</strong> {cartoon?.Description}\n" +
                 $"<strong>💸 Budget:</strong> {cartoon?.Budget}\n",
                 parseMode: ParseMode.Html,
                 replyMarkup: new InlineKeyboardMarkup(
         InlineKeyboardButton.WithUrl("👉 Check out the cartoon", cartoon.CartoonUrl)),
                 cancellationToken);

            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
    }

    private async Task GetFoundMoviesAsync(string searchString, long chatId, CancellationToken cancellationToken)
    {
        var tasks = new List<Task>();
        try
        {
            var movies = await _movieService.GetMoviesByTitleAsync(searchString, _moviePageByTitle);

            if (movies != null && movies.Any())
            {
                await SendMoviesAsync(movies, chatId, cancellationToken);
                await SendMoviesByTitleNavAsync(searchString, chatId, cancellationToken);
            }
        }
        catch (KeyNotFoundException ex)
        {
            var sendTextMessageAsync = _botService.SendTextMessageAsync(chatId, ex.Message, cancellationToken: cancellationToken);
            _logger.LogInformation($"User chat id: {chatId} does not found a movies with a title: {searchString}");
            tasks.Add(sendTextMessageAsync);
        }
        catch (Exception ex)
        {
            await _botService.SendTextMessageAsync(chatId, "An error occurred while searching for movies.", cancellationToken: cancellationToken);
            await SendMoviesNavAsync(chatId, cancellationToken);
            _logger.LogInformation($"Application has an other mistakes like: {ex.Message}");
        }
    }

    private void IncrementMoviePage() => ++_moviePage;

    private void DecrementMoviePage() => --_moviePage;

    private void IncrementMoviePageByTitle() => ++_moviePageByTitle;

    private void DecrementMoviePageByTitle() => --_moviePageByTitle;

    private void IncrementCartoonPage() => ++_cartoonPage;

    private void DecrementCartoonPage() => --_cartoonPage;
}
