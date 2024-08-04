using Microsoft.Extensions.Logging;
using MoviesTelegramBotApp.Interfaces;
using MoviesTelegramBotApp.Models;
using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

/// <summary>
/// Handles incoming updates from Telegram by processing user messages and updating their state accordingly.
/// </summary>
/// <remarks>
/// The class is responsible for managing different user states, such as awaiting movie search or navigating through movie results.
/// It interacts with the Telegram bot client to send messages and updates based on user input and state.
/// </remarks>
internal class UpdateHandler
{
    private readonly IBotService _botService;
    private readonly IMovieService _movieService;
    private readonly ICartoonService _cartoonService;
    private readonly ConcurrentDictionary<long, (string state, string searchString)> _userStates;
    private readonly ConcurrentDictionary<long, UserState> _userGenreState;
    private readonly ILogger<UpdateHandler> _logger;
    private int _moviePage = 1;
    private int _moviePageByTitle = 1;
    private int _moviePageByFavorite = 1;
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
        _userGenreState = new ConcurrentDictionary<long, UserState>();
        _cartoonService = cartoonService;
        _userStates = new ConcurrentDictionary<long, (string state, string searchString)>();
        _logger = logger;
    }

    /// <summary>
    /// Asynchronously handles incoming updates from Telegram, processing user messages based on their state.
    /// </summary>
    /// <param name="bot">The Telegram bot client used to interact with the user.</param>
    /// <param name="update">The update received from Telegram, containing message information.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <remarks>
    /// This method processes user messages based on their current state:
    /// - If the user is awaiting movie search, it prompts for a movie title or performs a search based on the provided title.
    /// - If the user is navigating movie results, it handles pagination ("Next" and "Previous") and menu navigation.
    /// - If the user sends the "/start" command, it sends a welcome message and displays the main menu.
    /// - For other messages, it handles menu responses accordingly.
    /// </remarks>
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
                        await _botService.SendTextMessageAsync(chatId, "🎬 Please, enter a title of movie", cancellationToken);

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

    /// <summary>
    /// Asynchronously sends a menu with options to the user in the specified chat.
    /// </summary>
    /// <param name="chatId">The ID of the chat where the menu will be sent.</param>
    /// <param name="cts">A cancellation token to observe while waiting for the task to complete.</param>
    /// <remarks>
    /// This method creates a reply keyboard with options such as "Movies," "Cartoons," "Surprise Me," and "Search." 
    /// It then sends a message to the user with this menu, allowing them to choose from these options. 
    /// The menu is displayed with a resizeable keyboard for ease of use.
    /// </remarks>
    private async Task SendMenuAsync(long chatId, CancellationToken cts)
    {
        var replyKeyBoardMarkup = new ReplyKeyboardMarkup(new[] { new KeyboardButton[] { "🎥 Movies", "🎞️ Cartoons", "✨ Surprise Me", "🔎 Search", "🎞️ Choices" } }) { ResizeKeyboard = true };

        await _botService.SendTextMessageAsync(
            chatId,
            "Choose an option, please: 🔽",
            parseMode: ParseMode.Html,
            replyKeyBoardMarkup,
            cancellationToken: cts);
    }

    /// <summary>
    /// Asynchronously sends navigation buttons for movies based on the current page and total number of movies.
    /// </summary>
    /// <param name="chatId">The ID of the chat where the navigation buttons will be sent.</param>
    /// <param name="cts">A cancellation token to observe while waiting for the task to complete.</param>
    /// <remarks>
    /// This method calculates whether to show "Previous" and "Next" buttons based on the current page and total movie count. 
    /// It then invokes <see cref="SendNavigationAsync"/> to send the navigation options to the user, including buttons for 
    /// returning to the main menu, viewing genres, and navigating to the previous or next movie.
    /// </remarks>
    private async Task SendMoviesNavAsync(long chatId, CancellationToken cts)
    {
        var totalMovies = await _movieService.CountAsync;
        bool showPrevious = _moviePage > 1;
        bool showNext = _moviePage < totalMovies;

        // todo: here I need to check If I have a list of choosed items

        await SendNavigationAsync(chatId, cts, showPrevious, showNext, "Main Menu 🔝", "🎬 Genres", "⏮️ Prev Movie", "➕ Favorite", "Next Movie ⏭️");
    }

    /// <summary>
    /// Asynchronously sends navigation buttons for movies of a specified genre to the user.
    /// </summary>
    /// <param name="genre">The genre of movies to navigate.</param>
    /// <param name="chatId">The chat identifier where the navigation buttons will be sent.</param>
    /// <param name="cts">CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method retrieves the total number of movies for the user's current genre and page. 
    /// It then determines whether to show previous and next navigation buttons based on the current page and total number of movies.
    /// Finally, it sends the navigation buttons to the user.
    /// </remarks>
    private async Task SendMoviesNavByGenreAsync(string genre, long chatId, CancellationToken cts)
    {
        if (_userGenreState.TryGetValue(chatId, out var userGenreState))
        {
            if (!string.IsNullOrEmpty(userGenreState.CurrentGenre))
            {
                var totalMovies = await _movieService.GetMoviesByGenreAsync(userGenreState.CurrentGenre, userGenreState.CurrentPage);

                bool showPrevious = userGenreState.CurrentPage > 1;
                bool showNext = userGenreState.CurrentPage < totalMovies.Count;

                await SendNavigationAsync(chatId, cts, showPrevious, showNext, "🎬 Genres", string.Empty, "⏮️ Show Prev", string.Empty, "Show Next ⏭️");
            }
        }
    }

    /// <summary>
    /// Asynchronously sends navigation buttons for movies based on a search title to the user.
    /// </summary>
    /// <param name="searchString">The title used to search for movies.</param>
    /// <param name="chatId">The chat identifier to send the message to.</param>
    /// <param name="cts">A cancellation token to cancel the operation if needed.</param>
    /// <remarks>
    /// The method determines whether to show "Previous" and "Next" buttons based on the current page and the total number of movies found.
    /// It then sends these navigation options to the user, allowing them to navigate through the list of movies that match the search title.
    /// </remarks>
    private async Task SendMoviesByTitleNavAsync(string searchString, long chatId, CancellationToken cts)
    {
        var totalMovies = await _movieService.GetMoviesByTitleCountAsync(searchString);
        bool showPrevious = _moviePageByTitle > 1;
        bool showNext = _moviePageByTitle < totalMovies;

        await SendNavigationAsync(chatId, cts, showPrevious, showNext, "Main Menu 🔝", string.Empty, "⏮️ Prev", string.Empty, "Next ⏭️");
    }

    private async Task SendChoicesMoviesNavAsync(long chatId, CancellationToken cts)
    {
        var totalMovies = await _movieService.GetListOfFavoriteMoviesAsync(_moviePage);
        var count = totalMovies.Count;
        bool showPrevious = count > 1;
        bool showNext = _moviePageByFavorite < count;

        await SendNavigationAsync(chatId, cts, showPrevious, showNext, "Main Menu 🔝", string.Empty, "⏮️ Prev", string.Empty, "Next ⏭️");
    }

    private async Task SendCartoonsNavAsync(long chatId, CancellationToken cts)
    {
        var totalCartoons = await _cartoonService.Count;
        bool showPrevious = _cartoonPage > 1;
        bool showNext = _cartoonPage < totalCartoons;

        await SendNavigationAsync(chatId, cts, showPrevious, showNext, "Main Menu 🔝", string.Empty, "⏮️ Prev Cartoon", string.Empty, "Next Cartoon ⏭️");
    }

    /// <summary>
    /// Asynchronously sends a navigation keyboard to the user with buttons for navigating through a list, including options for previous and next items, a genre button, and a back button.
    /// </summary>
    /// <param name="chatId">The chat identifier to send the message to.</param>
    /// <param name="cts">A cancellation token to cancel the operation if needed.</param>
    /// <param name="showPrevious">Indicates whether the "Previous" button should be shown.</param>
    /// <param name="showNext">Indicates whether the "Next" button should be shown.</param>
    /// <param name="backButtonText">Text for the back button.</param>
    /// <param name="genre">Text for the genre button.</param>
    /// <param name="previousButtonText">Text for the previous button.</param>
    /// <param name="nextButtonText">Text for the next button.</param>
    /// <remarks>
    /// The method constructs a list of buttons based on whether navigation options are available and sends this keyboard layout to the user.
    /// It provides navigation controls for both the previous and next items, along with options to return to a genre selection or main menu.
    /// </remarks>
    private async Task SendNavigationAsync(long chatId,
        CancellationToken cts,
        bool showPrevious,
        bool showNext, string backButtonText, string genre,
        string previousButtonText, string favorite,
        string nextButtonText)
    {
        var buttons = new List<KeyboardButton[]>();

        if (showPrevious && showNext)
        {
            buttons.Add(new KeyboardButton[] { previousButtonText, favorite, nextButtonText });
            buttons.Add(new KeyboardButton[] { genre });
            buttons.Add(new KeyboardButton[] { backButtonText });
        }
        else if (showPrevious)
        {
            buttons.Add(new KeyboardButton[] { favorite, previousButtonText });
            buttons.Add(new KeyboardButton[] { genre });
            buttons.Add(new KeyboardButton[] { backButtonText });
        }
        else if (showNext)
        {
            buttons.Add(new KeyboardButton[] { favorite, nextButtonText });
            buttons.Add(new KeyboardButton[] { genre });
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

    /// <summary>
    /// Asynchronously sends a message with buttons for each movie genre to a specified chat.
    /// </summary>
    /// <param name="chatId">The ID of the chat to send the message to.</param>
    /// <param name="cts">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method retrieves all movie genres from the movie service and creates a keyboard with buttons for each genre. 
    /// It also includes a "Go Top 🔝" button. If the genres are unavailable, an error message is sent.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown if there is an issue retrieving genres from the database.</exception>
    private async Task SendMoviesGenresButtons(long chatId, CancellationToken cts)
    {
        var tasks = new List<Task>();

        try
        {
            var genres = await _movieService.GetAllGenresAsync();

            var buttons = new List<KeyboardButton>();

            foreach (var genre in genres)
            {
                var button = new KeyboardButton($"{"🧾 " + genre.Name}");

                buttons.Add(button);
            }

            buttons.Add(new KeyboardButton("🎥 Movies"));

            KeyboardButton[] buttonArray = buttons.ToArray();
            var replyKeyBoardMarkup = new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };

            var sendTextMessageAsync = _botService.SendTextMessageAsync(
                chatId,
                "<b>Сhoose a movie genre, please 🔽</b>",
                parseMode: ParseMode.Html,
                replyKeyBoardMarkup,
                cancellationToken: cts);
            tasks.Add(sendTextMessageAsync);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogCritical(ex.Message);
            var exResponse = "Sorry, the genres are not available 😟";
            var sendTextMessageAsync = _botService.SendTextMessageAsync(chatId, exResponse, cancellationToken: cts);
            tasks.Add(sendTextMessageAsync);
        }
        finally
        {
            await Task.WhenAll(tasks);
        }
    }

    /// <summary>
    /// Asynchronously handles user responses to menu options and navigates to the appropriate content based on the user's selection.
    /// </summary>
    /// <param name="chatId">The chat identifier where the response is being handled.</param>
    /// <param name="messageText">The text of the user's message, indicating their menu choice.</param>
    /// <param name="cancellationToken">A cancellation token to handle the cancellation of the operation if needed.</param>
    /// <remarks>
    /// The method processes different types of user responses related to movies and cartoons, including genre selections, navigation actions, and search requests. 
    /// It updates the user's state accordingly and sends appropriate responses, such as lists of movies or cartoons, navigation buttons, or prompts for search input.
    /// Specific actions include fetching movies or cartoons, updating pagination, and redirecting to the main menu or other menu options based on user input.
    /// </remarks>
    private async Task HandleMenuResponseAsync(long chatId, string messageText, CancellationToken cancellationToken)
    {
        switch (messageText)
        {
            case "🎥 Movies":
                await GetMoviesAsync(chatId, cancellationToken);
                await SendMoviesNavAsync(chatId, cancellationToken);
                break;

            // genres
            case "🎬 Genres":
                await SendMoviesGenresButtons(chatId, cancellationToken);
                break;

            case "🧾 Action":
                if (!_userGenreState.ContainsKey(chatId))
                {
                    _userGenreState[chatId] = new UserState();
                }

                _userGenreState[chatId].CurrentGenre = "Action";
                _userGenreState[chatId].CurrentPage = 1;
                await GetAllMoviesByGenre(chatId, cancellationToken);
                await SendMoviesNavByGenreAsync("Action", chatId, cancellationToken);
                break;

            case "🧾 Comedy":
                if (!_userGenreState.ContainsKey(chatId))
                {
                    _userGenreState[chatId] = new UserState();
                }

                _userGenreState[chatId].CurrentGenre = "Comedy";
                _userGenreState[chatId].CurrentPage = 1;
                await GetAllMoviesByGenre(chatId, cancellationToken);
                await SendMoviesNavByGenreAsync("Comedy", chatId, cancellationToken);
                break;

            case "🧾 Drama":
                if (!_userGenreState.ContainsKey(chatId))
                {
                    _userGenreState[chatId] = new UserState();
                }

                _userGenreState[chatId].CurrentGenre = "Drama";
                _userGenreState[chatId].CurrentPage = 1;
                await GetAllMoviesByGenre(chatId, cancellationToken);
                await SendMoviesNavByGenreAsync("Drama", chatId, cancellationToken);
                break;

            case "Show Next ⏭️":
                var tasks = new List<Task>();
                if (_userGenreState.TryGetValue(chatId, out var userGenreState))
                {
                    if (!string.IsNullOrEmpty(userGenreState.CurrentGenre))
                    {
                        userGenreState.CurrentPage++;
                        await GetAllMoviesByGenre(chatId, cancellationToken);
                        await SendMoviesNavByGenreAsync(userGenreState.CurrentGenre, chatId, cancellationToken);
                    }
                }
                else
                {
                    await _botService.SendTextMessageAsync(chatId, "Please, select a genre first", cancellationToken);
                }
                break;

            case "⏮️ Show Prev":

                if (_userGenreState.TryGetValue(chatId, out var userGenState))
                {
                    if (!string.IsNullOrEmpty(userGenState.CurrentGenre))
                    {
                        userGenState.CurrentPage--;
                        await GetAllMoviesByGenre(chatId, cancellationToken);
                        await SendMoviesNavByGenreAsync(userGenState.CurrentGenre, chatId, cancellationToken);
                    }
                }
                else
                {
                    await _botService.SendTextMessageAsync(chatId, "Please, select a genre first", cancellationToken);
                }
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

            case "➕ Favorite":
                await UpdateIsFavoriteAsync(chatId, cancellationToken, _moviePageByFavorite, true);
                await SendMoviesNavAsync(chatId, cancellationToken);
                break;

            case "🎞️ Choices":
                await GetListOfFavoriteMoviesAsync(chatId, cancellationToken);
                await SendChoicesMoviesNavAsync(chatId, cancellationToken);
                break;

            case "Next ⏭️":
                IncrementMoviePageByFavorite();
                await GetListOfFavoriteMoviesAsync(chatId, cancellationToken);
                await SendChoicesMoviesNavAsync(chatId, cancellationToken);
                break;

            case "⏮️ Prev":
                DecrementMoviePageByFavorite();
                await GetListOfFavoriteMoviesAsync(chatId, cancellationToken);
                break;

            case "🔎 Search":
                await _botService.SendTextMessageAsync(chatId, "🔍 Please enter a movie you want to find\n  For example: 'The Mask'", ParseMode.Html);
                _userStates[chatId] = (StateAwaitingMovieSearch, string.Empty);
                break;

            case "Go Top 🔝":
                await GetMoviesAsync(chatId, cancellationToken);
                await SendMoviesNavAsync(chatId, cancellationToken);
                break;

            case "Main Menu 🔝":
                await SendMenuAsync(chatId, cancellationToken);
                break;
        }
    }

    /// <summary>
    /// Asynchronously retrieves a list of movies and sends them to the user.
    /// </summary>
    /// <param name="chatId">The chat identifier where the movies will be sent.</param>
    /// <param name="cancellationToken">A cancellation token to handle the cancellation of the operation if needed.</param>
    /// <remarks>
    /// The method performs the following operations:
    /// 1. Fetches movies for the current page from the movie service.
    /// 2. Sends the retrieved movies to the user.
    /// 3. Handles any exceptions that occur during the process by logging the error and notifying the user of the service unavailability.
    /// It uses a list of tasks to manage the asynchronous operations and ensures all tasks are completed before finishing.
    /// </remarks>
    private async Task GetMoviesAsync(long chatId, CancellationToken cancellationToken)
    {
        var tasks = new List<Task>();

        try
        {
            var moviesTask = _movieService.GetAllMoviesAsync(_moviePage);
            tasks.Add(moviesTask);

            var getAllMoviesAsync = await moviesTask;

            var sendMoviesAsync = SendMoviesAsync(getAllMoviesAsync, chatId, cancellationToken);
            tasks.Add(sendMoviesAsync);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex.Message);
            var exResponse = "We are sorry.\nService is unavailable. We do our best to resolve this mistake. 😟";
            var sendTextMessageAsync = _botService.SendTextMessageAsync(chatId, exResponse, cancellationToken: cancellationToken);
            tasks.Add(sendTextMessageAsync);
        }

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Asynchronously retrieves a random movie and sends it to the user.
    /// </summary>
    /// <param name="chatId">The chat identifier where the movie will be sent.</param>
    /// <param name="cts">A cancellation token to handle the cancellation of the operation if needed.</param>
    /// <remarks>
    /// The method performs the following operations:
    /// 1. Requests a random movie from the movie service.
    /// 2. Sends the retrieved movie to the user.
    /// 3. Handles any exceptions by logging the error and notifying the user that the movie is not available.
    /// 4. Ensures all asynchronous operations are completed before finishing.
    /// </remarks>
    private async Task GetRandomMovieAsync(long chatId, CancellationToken cts)
    {
        var tasks = new List<Task>();

        try
        {
            var rndMovieTask = _movieService.GetRandomMovieAsync();
            tasks.Add(rndMovieTask);
            var getRandomMovieAsync = await rndMovieTask;
            var sendMoviesTask = SendMoviesAsync(getRandomMovieAsync, chatId, cts);
            tasks.Add(sendMoviesTask);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex.Message);
            var sendTextMessageAsync = _botService.SendTextMessageAsync(chatId, "Sorry, the movie is not available. 😟");
            tasks.Add(sendTextMessageAsync);
        }
        finally
        {
            await Task.WhenAll(tasks);
        }
    }

    /// <summary>
    /// Asynchronously sends a list of movies to a user, including their details and an inline button for each movie.
    /// </summary>
    /// <param name="movies">The collection of movies to send.</param>
    /// <param name="chatId">The chat ID of the user to whom the movies will be sent.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
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

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            await _botService.SendTextMessageAsync(chatId, "Sorry, something went wrong. Try again later. 😟", cancellationToken);
            _logger.LogCritical($"An exception '{ex.Message}' occurred during sending movies to user.");
        }
        finally
        {
            _logger.LogInformation("Finished processing movie send requests.");
        }
    }

    /// <summary>
    /// Asynchronously sends a single movie's details to a user on Telegram, including an image, a detailed caption, and an inline button for a movie trailer.
    /// </summary>
    /// <param name="movie">The movie object containing details to be sent.</param>
    /// <param name="chatId">The chat ID of the user to whom the movie will be sent.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
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

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            await _botService.SendTextMessageAsync(chatId, "Sorry, something went wrong. Try again later.", cancellationToken);
            _logger.LogCritical($"An exception '{ex.Message}' occurred during sending movies to user.");
        }
        finally
        {
            _logger.LogInformation("Finished processing movie send requests.");
        }
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

    /// <summary>
    /// Asynchronously retrieves movies based on a search string, sends the movie details to the specified chat, and handles any errors that occur during the process.
    /// </summary>
    /// <param name="searchString">The title of the movie to search for.</param>
    /// <param name="chatId">The chat ID where the results will be sent.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation if needed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task GetFoundMoviesAsync(string searchString, long chatId, CancellationToken cancellationToken)
    {
        var tasks = new List<Task>();

        try
        {
            var moviesTask = _movieService.GetMoviesByTitleAsync(searchString, _moviePageByTitle);
            tasks.Add(moviesTask);

            var movies = await moviesTask;

            if (movies != null && movies.Any())
            {
                var sendMoviesAsync = SendMoviesAsync(movies, chatId, cancellationToken);
                tasks.Add(sendMoviesAsync);

                var sendMoviesByTitleNavAsync = SendMoviesByTitleNavAsync(searchString, chatId, cancellationToken);
                tasks.Add(sendMoviesByTitleNavAsync);
            }
        }
        catch (KeyNotFoundException ex)
        {
            string errMessage = $"Couldn't find movie by title: '{searchString}'.\nWould you like to try searching with another title?";
            await _botService.SendTextMessageAsync(chatId, errMessage, cancellationToken: cancellationToken);

            _logger.LogInformation($"{ex.Message}");
        }
        catch (Exception ex)
        {
            await _botService.SendTextMessageAsync(chatId, "Sorry, the movie is not available 😟.", cancellationToken: cancellationToken);
            await SendMoviesNavAsync(chatId, cancellationToken);

            _logger.LogInformation($"Application has other mistakes like: {ex.Message}");
        }
        finally
        {
            foreach (var task in tasks)
            {
                try
                {
                    await task;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occurred while processing a task: {ex.Message}");
                    await _botService.SendTextMessageAsync(chatId, "An error occurred while processing your request. Please try again later.", cancellationToken);
                }
            }

            _logger.LogInformation("Finished processing found movies request.");
        }
    }

    /// <summary>
    /// Asynchronously retrieves and sends a list of movies by genre for a specified user and chat.
    /// </summary>
    /// <param name="genre">The genre of movies to retrieve.</param>
    /// <param name="chatId">The chat identifier where the movies will be sent.</param>
    /// <param name="cts">CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the genre is null or empty.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if no movies are found for the specified genre.</exception>
    /// <remarks>
    /// This method retrieves the user's current page for the specified genre and attempts to send the list of movies. 
    /// If an error occurs, appropriate messages are sent to the user and logged.
    /// </remarks>
    private async Task GetAllMoviesByGenre(long chatId, CancellationToken cts)
    {
        if (_userGenreState.TryGetValue(chatId, out var genreUserState))
        {
            if (!string.IsNullOrEmpty(genreUserState.CurrentGenre))
            {
                var tasks = new List<Task>();

                try
                {
                    var getMoviesByGenre = _movieService.GetMoviesByGenreAsync(genreUserState.CurrentGenre, genreUserState.CurrentPage);
                    tasks.Add(getMoviesByGenre);

                    var moviesByGenre = await getMoviesByGenre;

                    var sendMoviesAsync = SendMoviesAsync(moviesByGenre.Movies, chatId, cts);
                    tasks.Add(sendMoviesAsync);
                }
                catch (ArgumentNullException ex)
                {
                    await _botService.SendTextMessageAsync(chatId, "Please, enter a movies genre", cts);
                    _logger.LogWarning($"ArgumentNullException: {ex.Message}");
                }
                catch (KeyNotFoundException ex)
                {
                    await _botService.SendTextMessageAsync(chatId, $"Sorry, the movies by genre {genreUserState.CurrentGenre} is not available 😟.", cts);
                    _logger.LogError($"KeyNotFoundException: {ex.Message}");
                }
                catch (Exception ex)
                {
                    await _botService.SendTextMessageAsync(chatId, "An error occurred while processing your request. Please try again later.", cts);
                    _logger.LogError($"Exception: {ex.Message}");
                }
                finally
                {
                    foreach (var task in tasks)
                    {
                        try
                        {
                            await task;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"An error occurred while processing a task: {ex.Message}");
                            await _botService.SendTextMessageAsync(chatId, "An error occurred while processing your request. Please try again later.", cts);
                        }
                    }

                    _logger.LogInformation("Finished processing movies by genre request.");
                }
            }
        }
    }

    /// <summary>
    /// Asynchronously updates the 'IsFavorite' status of a movie and sends a confirmation message to the specified chat.
    /// Handles and logs errors if the update fails, sending an appropriate error message to the chat.
    /// </summary>
    /// <param name="chatId">The ID of the chat to send messages to.</param>
    /// <param name="cts">The cancellation token to handle task cancellation.</param>
    /// <param name="movieId">The ID of the movie to update.</param>
    /// <param name="isFavorite">The new favorite status of the movie.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// 
    private async Task UpdateIsFavoriteAsync(long chatId, CancellationToken cts, int movieId, bool isFavorite)
    {
        var taskList = new List<Task>();
        try
        {
            var updateIsFavorite = _movieService.UpdateIsFavoriteAsync(movieId, isFavorite);
            taskList.Add(updateIsFavorite);

            await updateIsFavorite;

            await _botService.SendTextMessageAsync(chatId, "The movie was added to Choices list", cts);
        }
        catch (NullReferenceException ex)
        {
            await _botService.SendTextMessageAsync(chatId, "Sorry, an error occurred while adding the movie to favorites", cts);
            _logger.LogCritical($"An error occurred during finding the movie in database. See message: {ex.Message}");
        }
        catch (Exception ex)
        {
            await _botService.SendTextMessageAsync(chatId, "Sorry, an error occurred while adding the movie to favorites", cts);
            _logger.LogCritical($"There was an error updating the 'Is Favorite' property for this movie in the database. See message: {ex.Message}");
        }
        finally
        {
            await Task.WhenAll(taskList);
        }
    }

    /// <summary>
    /// Asynchronously retrieves a list of favorite movies and sends them to a specified chat.
    /// This method first fetches the list of favorite movies, then sends the list to the chat. 
    /// It handles and logs exceptions if an error occurs during retrieval or sending of the movie list.
    /// </summary>
    /// <param name="chatId">The ID of the chat to which the movies will be sent.</param>
    /// <param name="cts">A cancellation token to cancel the sending operation if needed.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task GetListOfFavoriteMoviesAsync(long chatId, CancellationToken cts)
    {
        var tasksList = new List<Task>();

        try
        {
            var getAllFavoriteMovies = _movieService.GetListOfFavoriteMoviesAsync(_moviePageByFavorite);
            tasksList.Add(getAllFavoriteMovies);

            var favMovies = await getAllFavoriteMovies;

            var sendMovies = SendMoviesAsync(favMovies.Movies, chatId, cts);
            tasksList.Add(sendMovies);
        }
        catch (NullReferenceException ex)
        {
            await _botService.SendTextMessageAsync(chatId, "Sorry, an error occurred while retrieving movies from favorite list", cts);
            _logger.LogCritical($"An error occurred during retrieving movies from favorite list. See message: {ex.Message}");
        }
        catch (Exception ex)
        {
            await _botService.SendTextMessageAsync(chatId, "Sorry, an error occurred while retrieving movies from favorite list", cts);
            _logger.LogCritical($"An error occurred during retrieving movies from favorite list. See message: {ex.Message}");
        }

        finally
        {
            await Task.WhenAll(tasksList);
        }
    }

    /// <summary>
    /// Increments the current page number for retrieving movies.
    /// </summary>
    private void IncrementMoviePage() => ++_moviePage;

    /// <summary>
    /// Decrement the current page number for retrieving movies.
    /// </summary>
    private void DecrementMoviePage() => --_moviePage;

    /// <summary>
    /// Increments the current page number by title for retrieving movies.
    /// </summary>
    private void IncrementMoviePageByTitle() => ++_moviePageByTitle;

    /// <summary>
    /// Decrements the current page number by title for retrieving movies.
    /// </summary>
    private void DecrementMoviePageByTitle() => --_moviePageByTitle;

    /// <summary>
    /// Increments the current page number by title for retrieving movies.
    /// </summary>
    private void IncrementMoviePageByFavorite() => ++_moviePageByFavorite;

    /// <summary>
    /// Decrements the current page number by title for retrieving movies.
    /// </summary>
    private void DecrementMoviePageByFavorite() => --_moviePageByFavorite;

    /// <summary>
    /// Increments the current page number for retrieving cartoons.
    /// </summary>
    private void IncrementCartoonPage() => ++_cartoonPage;

    /// <summary>
    /// Decrements the current page number for retrieving cartoons.
    /// </summary>
    private void DecrementCartoonPage() => --_cartoonPage;
}
