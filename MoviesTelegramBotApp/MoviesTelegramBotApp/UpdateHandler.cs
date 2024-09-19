using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MoviesTelegramBotApp.Interfaces;
using MoviesTelegramBotApp.Models;
using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
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
internal class UpdateHandler : IUpdateHandler
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
    private readonly long _adminChatId;

    private const string StateAwaitingMovieSearch = "awaiting_movie_search";
    private const string StateNavigatingMovies = "navigating_movies";

    public UpdateHandler(
        IBotService botService,
        IMovieService movieService,
        ICartoonService cartoonService,
        ILogger<UpdateHandler> logger,
        IConfiguration configuration)
    {
        _botService = botService;
        _movieService = movieService;
        _userGenreState = new ConcurrentDictionary<long, UserState>();
        _cartoonService = cartoonService;
        _userStates = new ConcurrentDictionary<long, (string state, string searchString)>();
        _logger = logger;
        _adminChatId = long.Parse(configuration["AdminChatId"]!);
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
                    await GetFoundMoviesAsync(messageText!, chatId, cancellationToken);                    
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
        var totalMovies = await _movieService.GetAllMoviesAsync(_moviePage);
        bool showPrevious = _moviePage > 1;
        bool showNext = _moviePage < totalMovies.Count;

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
        var totalMovies = await _movieService.GetMoviesByTitleAsync(searchString, _moviePageByTitle);
        bool showPrevious = _moviePageByTitle > 1;
        bool showNext = _moviePageByTitle < totalMovies.Count;

        await SendNavigationAsync(chatId, cts, showPrevious, showNext, "Main Menu 🔝", string.Empty, "⏮️ Prev", string.Empty, "Next ⏭️");
    }

    private async Task SendChoicesMoviesNavAsync(long chatId, CancellationToken cts)
    {
        var movies = await _movieService.GetListOfFavoriteMoviesAsync(chatId, _moviePageByFavorite);        
        bool showPrevious = _moviePageByFavorite > 1;
        bool showNext = _moviePageByFavorite < movies.Count;

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
    /// Retrieves movie genres from the service and sends a list of genre buttons to the specified chat. 
    /// If no genres are found, a fallback message is sent. In case of an error, an error message is sent.
    /// </summary>
    /// <param name="chatId">The ID of the chat to which the genre buttons or error message will be sent.</param>
    /// <param name="cts">A cancellation token to observe while sending messages asynchronously.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task SendMoviesGenresButtons(long chatId, CancellationToken cts)
    {
        var tasks = new List<Task>();

        try
        {
            var genresTask = _movieService.GetAllGenresAsync();
            tasks.Add(genresTask);

            var genres = await genresTask;

            _logger.LogInformation($"Retrieved {genres.Count} genres for chat ID {chatId}.");

            if (!genres.Any())
            {
                _logger.LogWarning("No genres found. Sending fallback message.");
                tasks.Add(_botService.SendTextMessageAsync(
                    chatId,
                    "No genres found. Please try again later.",
                    cancellationToken: cts));
            }
            else
            {
                var buttons = genres
                    .Select(genre => new KeyboardButton($"🧾 {genre.Name}"))
                    .Append(new KeyboardButton("🎥 Movies"))
                    .ToArray();

                var replyKeyboardMarkup = new ReplyKeyboardMarkup(buttons)
                {
                    ResizeKeyboard = true
                };

                tasks.Add(_botService.SendTextMessageAsync(
                    chatId,
                    "<b>Choose a movie genre, please 🔽</b>",
                    parseMode: ParseMode.Html,
                    replyKeyboardMarkup,
                    cancellationToken: cts));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred while sending genre buttons for chat ID {chatId}.", ex);
            tasks.Add(_botService.SendTextMessageAsync(
                chatId,
                "Sorry, the genres are not available 😟",
                cancellationToken: cts));
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
                await UpdateIsFavoriteAsync(chatId, cancellationToken, _moviePage, true);
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
                await SendChoicesMoviesNavAsync(chatId, cancellationToken);
                break;

            case "🔎 Search":
                 await _botService.SendTextMessageAsync(chatId, "🔍 Please enter a movie you want to find" +
                    "\nFor example: 'The Mask'", ParseMode.Html);
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
    private async Task GetMoviesAsync(long chatId, CancellationToken cancellationToken)
    {
        var tasks = new List<Task>();

        try
        {
            _logger.LogInformation($"Fetching movies for chatId {chatId} on page {_moviePage}.");

            var moviesTask = _movieService.GetAllMoviesAsync(_moviePage);
            tasks.Add(moviesTask);

            var data = await moviesTask;

            if (!data.Movies.Any())
            {
                _logger.LogInformation($"No movies found for chatId {chatId} on page {_moviePage}.");
                tasks.Add(_botService.SendTextMessageAsync(chatId, "Something terrible happened... but no movies found.\n" +
                    " Please, try again later", cancellationToken));
            }
            else
            {
                _logger.LogInformation($"{data.Movies.Count} movies found for chatId {chatId}.");
                tasks.Add(SendMoviesAsync(data.Movies, chatId, cancellationToken));
            }
        }
        catch (ArgumentOutOfRangeException ex)
        {
            _logger.LogCritical(ex, $"Invalid page number during movie retrieval for chatId {chatId}. Exception: {ex.Message}");
            tasks.Add(_botService.SendTextMessageAsync(chatId,
                "We are sorry.\nService is unavailable. We do our best to resolve this mistake. 😟",
                cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Unexpected error while retrieving movies for chatId {chatId}. Exception: {ex.Message}");
            var exResponse = "We are sorry.\nService is unavailable. We do our best to resolve this mistake. 😟";
            tasks.Add(_botService.SendTextMessageAsync(chatId, exResponse, cancellationToken));
        }
        finally
        {
            _logger.LogInformation($"Finished processing movie retrieval for chatId {chatId}.");
            await Task.WhenAll(tasks);
        }
    }

    /// <summary>
    /// Fetches a random movie from the movie service and sends it to the specified chat. 
    /// If no movie is found, or an error occurs, logs the issue and notifies the user.
    /// </summary>
    /// <param name="chatId">The ID of the chat to which the movie details will be sent.</param>
    /// <param name="cts">CancellationToken to signal task cancellation.</param>
    /// <returns>Task representing the asynchronous operation of fetching and sending a random movie.</returns>
    /// <remarks>
    /// If no movie is returned, a warning is logged and the user is informed that no movie is available. 
    /// If an exception occurs, it is logged as critical, and an error message is sent to the user.
    /// </remarks>
    private async Task GetRandomMovieAsync(long chatId, CancellationToken cts)
    {
        var tasks = new List<Task>();

        try
        {
            var rndMovieTask = _movieService.GetRandomMovieAsync();
            tasks.Add(rndMovieTask);
            var getRandomMovieAsync = await rndMovieTask;

            if (getRandomMovieAsync == null)
            {
                _logger.LogWarning("No movie returned from GetRandomMovieAsync.");
                tasks.Add(_botService.SendTextMessageAsync(chatId, "Sorry, no movie is available at the moment. 😟", cancellationToken: cts));
                return;
            }
            tasks.Add(SendMoviesAsync(getRandomMovieAsync, chatId, cts));
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "An error occurred while fetching a random movie.");
            tasks.Add(_botService.SendTextMessageAsync(chatId, "Sorry, the movie is not available. 😟"));
        }
        finally
        {
            await Task.WhenAll(tasks);
        }
    }

    /// <summary>
    /// Sends a series of movie messages with inline buttons to a specified chat. Each movie is sent as a photo message with a caption and a button linking to the movie's trailer.
    /// </summary>
    /// <param name="movies">An enumerable collection of movies to be sent.</param>
    /// <param name="chatId">The ID of the chat where the messages will be sent.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation if needed.</param>
    /// <exception cref="Exception">Throws an exception if an error occurs during the message sending process.</exception>
    /// <remarks>
    /// The method logs errors if any exceptions occur during the process and rethrows the exception for further handling.
    /// </remarks>
    private async Task SendMoviesAsync(IEnumerable<Movie> movies, long chatId, CancellationToken cancellationToken)
    {
        try
        {
            await Task.WhenAll(movies.Select(movie =>
           _botService.SendPhotoWithInlineButtonUrlAsync(
              chatId,
              photoUrl: new Telegram.Bot.Types.InputFiles.InputOnlineFile(movie.ImageUrl),
              caption: BuildMovieCaption(movie),
              parseMode: ParseMode.Html,
              replyMarkup: new InlineKeyboardMarkup(
              InlineKeyboardButton.WithUrl("Check out the trailer", movie.MovieUrl)),
              cancellationToken)));

            _logger.LogInformation($"Successfully sent movie messages to chat ID {chatId}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while sending movie messages to chat ID {chatId}.");
            throw;
        }
    }

    /// <summary>
    /// Constructs a formatted caption for a movie, including details such as title, genre, description, country, budget, interest facts, and behind-the-scenes information.
    /// </summary>
    /// <param name="movie">The <see cref="Movie"/> object containing the details to be included in the caption.</param>
    /// <returns>A string representing the formatted caption for the movie, with HTML markup for styling.</returns>
    /// <remarks>
    /// The caption is designed for use in Telegram messages, leveraging HTML tags to format text and include relevant movie details.
    /// </remarks>
    private string BuildMovieCaption(Movie movie)
    {
        return $"<strong>▶️ Title:</strong> {movie.Title}\n" +
             $"<strong>🎬 Genre:</strong> {movie.Genre.Name}\n" +
             $"<strong>🧾 Description:</strong> {movie.Description}\n" +
             $"<strong>🌍 Country:</strong> {movie.Country}\n" +
             $"<strong>💸 Budget:</strong> {movie.Budget}\n" +
             $"<strong>📌 Interest facts:</strong> {movie.InterestFactsUrl}\n" +
             $"<strong>✨ Behind the scene:</strong> {movie.BehindTheScene}\n";
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
    /// Searches for movies based on the provided title and sends the results to the user in a chat. 
    /// If no movies are found, it prompts the user to enter a new search term. 
    /// The method also handles navigation between multiple results and updates the user's state for future interactions.
    /// Logs search attempts, results, and errors during the process.
    /// </summary>
    /// <param name="searchString">The title or part of the title to search for. Cannot be null or empty.</param>
    /// <param name="chatId">The chat ID of the user requesting the search.</param>
    /// <param name="cancellationToken">Token to propagate notification that the operation should be canceled.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the movie page number is invalid during search.</exception>
    /// <exception cref="ArgumentException">Thrown when the search string is invalid.</exception>
    /// <exception cref="Exception">Catches and logs any unhandled exceptions that occur during the operation and rethrows for global handling.</exception>
    private async Task GetFoundMoviesAsync(string searchString, long chatId, CancellationToken cancellationToken)
    {
        var tasks = new List<Task>();

        try
        {
            _logger.LogInformation($"Searching for movies with title '{searchString}' for chat {chatId}.");
            var moviesTask = _movieService.GetMoviesByTitleAsync(searchString, _moviePageByTitle);
            tasks.Add(moviesTask);

            var data = await moviesTask;

            if (!data.Movies.Any())
            {
                _logger.LogInformation($"No movies found for search '{searchString}' in chat {chatId}.");
                tasks.Add(_botService.SendTextMessageAsync(chatId, "I couldn't find any movies that match your search criteria." +
                    " 🔍 Please enter a new movie title to search for:", cancellationToken));
                _userStates[chatId] = (StateNavigatingMovies, searchString);
            }
            else
            {
                tasks.Add(SendMoviesAsync(data.Movies, chatId, cancellationToken));

                if (data.Count != 1)
                {
                    tasks.Add(SendMoviesByTitleNavAsync(searchString, chatId, cancellationToken));
                }
                _userStates[chatId] = (StateNavigatingMovies, searchString);               
            }
        }
        catch (ArgumentOutOfRangeException ex)
        {
            _logger.LogWarning(ex, $"Invalid movie page in search for '{searchString}' for chat {chatId}.");
            tasks.Add(_botService.SendTextMessageAsync(_adminChatId, $"Warning: {ex.Message}", cancellationToken));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, $"Invalid search string '{searchString}' in chat {chatId}.");
            tasks.Add(_botService.SendTextMessageAsync(chatId, "Invalid search input. Please try again.", cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unhandled exception during movie search for '{searchString}' in chat {chatId}.");
            tasks.Add(_botService.SendTextMessageAsync(chatId, "An error occurred while searching for movies. Please try again later.", cancellationToken));
            throw; // Rethrow to be caught by GlobalExceptionMiddleware            
        }
        finally
        {
            await Task.WhenAll(tasks);
        }
    }

    /// <summary>
    /// Retrieves movies by genre for a specific chat ID. The method checks if the user's genre state is valid and non-empty, then:
    /// 1. Logs the attempt to fetch movies for the specified genre and page.
    /// 2. Fetches the movies by genre using the `GetMoviesByGenreAsync` method.
    /// 3. Handles the result:
    ///    - If no movies are found, logs the event and informs the user to enter a new genre.
    ///    - If movies are found, logs the success and sends the movies to the user.
    /// 4. Catches and logs exceptions (`ArgumentOutOfRangeException`, `ArgumentException`, and general exceptions) while sending appropriate error messages to the user.
    /// 5. Ensures that all tasks are awaited and logs the completion of the request processing.
    ///
    /// This method ensures proper handling and logging of user requests for genre-specific movies, including error management and communication with the user.
    /// </summary>
    private async Task GetAllMoviesByGenre(long chatId, CancellationToken cts)
    {
        if (_userGenreState.TryGetValue(chatId, out var genreUserState))
        {
            if (!string.IsNullOrEmpty(genreUserState.CurrentGenre))
            {
                var tasks = new List<Task>();

                try
                {
                    _logger.LogInformation($"Fetching movies for genre '{genreUserState.CurrentGenre}'" +
                        $" on page {genreUserState.CurrentPage} for chat {chatId}.");

                    var getMoviesByGenre = _movieService.GetMoviesByGenreAsync(genreUserState.CurrentGenre,
                        genreUserState.CurrentPage);
                    tasks.Add(getMoviesByGenre);

                    var moviesByGenre = await getMoviesByGenre;

                    if (!moviesByGenre.Movies.Any())
                    {
                        _logger.LogInformation($"No movies found for genre '{genreUserState.CurrentGenre}' in chat {chatId}.");
                        tasks.Add(_botService.SendTextMessageAsync(chatId, "I couldn't find any movies that match your genre criteria." +
                            " 🔍 Please enter a new movie genre to search for:", cts));
                    }
                    else
                    {
                        _logger.LogInformation($"Successfully retrieved {moviesByGenre.Movies.Count} movies for genre '{genreUserState.CurrentGenre}'" +
                            $" on page {genreUserState.CurrentPage}.");
                        tasks.Add(SendMoviesAsync(moviesByGenre.Movies, chatId, cts));
                    }
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    _logger.LogError(ex, $"ArgumentOutOfRangeException: {ex.Message} - Issue retrieving movies for genre '{genreUserState.CurrentGenre}' for chat {chatId}.");
                    await _botService.SendTextMessageAsync(chatId, $"Sorry, the movies by genre '{genreUserState.CurrentGenre}' are not available. 😟", cts);
                }
                catch (ArgumentException ex)
                {
                    _logger.LogWarning(ex, $"ArgumentException: {ex.Message} - Invalid genre input '{genreUserState.CurrentGenre}' for chat {chatId}.");
                    await _botService.SendTextMessageAsync(chatId, "Please enter a valid movie genre.", cts);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Exception: {ex.Message} - An unexpected error occurred while processing movies for genre '{genreUserState.CurrentGenre}' for chat {chatId}.");
                    await _botService.SendTextMessageAsync(chatId, "An error occurred while processing your request. Please try again later.", cts);
                }
                finally
                {
                    await Task.WhenAll(tasks);
                    _logger.LogInformation($"Finished processing movies by genre request for genre '{genreUserState.CurrentGenre}' on page {genreUserState.CurrentPage} for chat {chatId}.");
                }
            }
            else
            {
                _logger.LogWarning($"Current genre is empty for chat {chatId}. Cannot retrieve movies.");
            }
        }
        else
        {
            _logger.LogWarning($"User genre state not found for chat {chatId}. Cannot retrieve movies.");
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
        var tasks = new List<Task>();
        try
        {
            var updateIsFavorite = _movieService.UpdateIsFavoriteAsync(chatId, movieId, isFavorite);
            tasks.Add(updateIsFavorite);

            await updateIsFavorite;
            await _botService.SendTextMessageAsync(chatId, "The movie was added to Choices list", cts);
        }
        catch (ArgumentOutOfRangeException ex)
        {

        }
        catch (Exception ex)
        {
            _logger.LogCritical($"There was an error updating the 'Is Favorite' property for this movie in the database. See message: {ex.Message}");
            await _botService.SendTextMessageAsync(chatId, "Sorry, an error occurred while adding or removing the movie to favorites", cts);
        }
        finally
        {
            await Task.WhenAll(tasks);
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
            var getAllFavoriteMovies = _movieService.GetListOfFavoriteMoviesAsync(chatId, _moviePageByFavorite);
            tasksList.Add(getAllFavoriteMovies);

            var favMovies = await getAllFavoriteMovies;

            tasksList.Add(SendMoviesAsync(favMovies.Movies, chatId, cts));
        }
        catch (ArgumentOutOfRangeException ex)
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

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException =>
            $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}"
        };

        _logger.LogError(errorMessage);

        return Task.CompletedTask;
    }
}
