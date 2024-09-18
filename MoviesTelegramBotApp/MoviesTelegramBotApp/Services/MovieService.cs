using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MoviesTelegramBotApp.Interfaces;
using MoviesTelegramBotApp.Models;

namespace MoviesTelegramBotApp.Services
{
    /// <summary>
    /// Provides methods for retrieving and managing movie data.
    /// </summary>
    public class MovieService : IMovieService
    {
        private ApplicationDbContext _dbContext;
        private readonly ILogger<MovieService> _logger;
        private readonly Random _random;
        public int PageSize = 1;

        public MovieService(ApplicationDbContext dbContext, ILogger<MovieService> logger, Random random)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger;
            _random = random;
        }

        /// <summary>
        /// Retrieves a paginated list of all movies from the database, including their associated genres.
        /// Logs relevant information, including the number of movies found or if none were found.
        /// Throws an exception if the specified movie page is less than 1 or if an error occurs during data retrieval.
        /// </summary>
        /// <param name="moviePage">The page number of the movie list to retrieve (must be 1 or greater).</param>
        /// <returns>A tuple containing a list of movies and the total count of movies in the database.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the movie page number is less than 1.</exception>
        /// <exception cref="Exception">Thrown when any error occurs while fetching movies from the database.</exception>
        public async Task<(List<Movie> Movies, int Count)> GetAllMoviesAsync(int moviePage = 1)
        {
            try
            {
                if (moviePage < 1)
                {
                    _logger.LogWarning($"Invalid movie page value {moviePage} in {nameof(GetAllMoviesAsync)}.");
                    throw new ArgumentOutOfRangeException(nameof(moviePage), "Movie page cannot be less than 1");
                }

                _logger.LogInformation($"Getting all movies form database on page {moviePage}.");

                var query = _dbContext.Movies
                .AsNoTracking()
                .Include(m => m.Genre);

                var totalCount = await query.CountAsync();

                var movies = await query
                    .OrderBy(m => m.Id)
                    .Skip((moviePage - 1) * PageSize)
                    .Take(PageSize)
                .ToListAsync();


                if (!movies.Any())
                {
                    _logger.LogInformation($"No movies found.");
                    return (Movies: new List<Movie>(), Count: totalCount);
                }

                _logger.LogInformation($"{movies.Count} movies found on page {moviePage}.");
                return (Movies: movies, Count: totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving movies on page {moviePage} in {nameof(GetAllMoviesAsync)}.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a paginated list of movies from the database that match the provided search string in their title.
        /// It performs a case-insensitive search and includes the genre information for each movie.
        /// Logs search attempts, results, and exceptions. Throws exceptions for invalid input.
        /// </summary>
        /// <param name="searchString">The title or part of the title to search for. Cannot be null or empty.</param>
        /// <param name="moviePage">The page number for pagination. Must be greater than or equal to 1.</param>
        /// <returns>A tuple containing a list of matching movies and the total count of movies found for the search term.</returns>
        /// <exception cref="ArgumentException">Thrown when the search string is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the movie page number is less than 1.</exception>
        /// <exception cref="Exception">Catches and logs any other unhandled exceptions that occur during the operation.</exception>
        public async Task<(List<Movie> Movies, int Count)> GetMoviesByTitleAsync(string searchString, int moviePage = 1)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchString))
                {
                    _logger.LogWarning($"Search string is null or empty in {nameof(GetMoviesByTitleAsync)}.");
                    throw new ArgumentException(nameof(searchString), "Search string in GetMoviesByTitleAsync cannot be null or empty.");
                }

                if (moviePage < 1)
                {
                    _logger.LogWarning($"Invalid movie page value {moviePage} in {nameof(GetMoviesByTitleAsync)}.");
                    throw new ArgumentOutOfRangeException(nameof(moviePage), "Movie page cannot be less than 1");
                }

                _logger.LogInformation($"Searching movies with title containing '{searchString}' on page {moviePage}.");

                var query = _dbContext.Movies
                    .AsNoTracking()
                    .Include(g => g.Genre)
                    .Where(m => m.Title != null && EF.Functions.Like(m.Title, $"%{searchString}%"));

                var movies = await query
                    .OrderBy(m => m.Id)
                    .Skip((moviePage - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                var totalCount = await query.CountAsync();

                if (!movies.Any())
                {
                    _logger.LogInformation($"No movies found for search term '{searchString}'.");
                    return (Movies: new List<Movie>(), Count: totalCount);
                }

                _logger.LogInformation($"{movies.Count} movies found for search term '{searchString}'.");
                return (Movies: movies, Count: totalCount);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in {nameof(GetMoviesByTitleAsync)}. Search string: {searchString}, Movie page: {moviePage}");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves a random movie from the collection of all movies.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing a randomly selected <see cref="Movie"/> object.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no movies are available in the collection.</exception>
        public async Task<Movie> GetRandomMovieAsync()
        {
            try
            {
                _logger.LogInformation("Fetching random movie from the database");

                var totalMoviesCount = await _dbContext.Movies.CountAsync();

                if (totalMoviesCount == 0)
                {
                    _logger.LogWarning("No movies found in the database");
                    return null;
                }

                int randomIndex = _random.Next(0, totalMoviesCount);

                var randomMovie = await _dbContext.Movies
                    .AsNoTracking()
                    .Include(m => m.Genre)
                    .Skip(randomIndex)
                    .FirstOrDefaultAsync();

                _logger.LogInformation($"Random movie '{randomMovie?.Title}' retrieved successfully.");

                return randomMovie;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving a random movie.");
                throw;
            }
        }
        /// <summary>
        /// Retrieves a paginated list of movies from the database that match a specified genre.
        /// </summary>
        /// <param name="genre">The genre to filter movies by. Must be a non-empty string.</param>
        /// <param name="moviePage">The page number for pagination. Must be greater than or equal to 1.</param>
        /// <returns>A tuple containing a list of movies that match the specified genre and the total count of such movies.</returns>
        /// <exception cref="ArgumentException">Thrown when the genre is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the moviePage is less than 1.</exception>
        /// <remarks>
        /// Logs warnings for invalid inputs and empty search results, and logs information about the search operation and results.
        /// If an exception occurs during the database query, it is logged and rethrown to be handled by higher-level exception handling mechanisms.
        /// </remarks>
        public async Task<(List<Movie> Movies, int Count)> GetMoviesByGenreAsync(string genre, int moviePage = 1)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(genre))
                {
                    _logger.LogWarning($"Search string is null or empty in {nameof(GetMoviesByGenreAsync)}.");
                    throw new ArgumentException(nameof(genre), "Search string genre in GetMoviesByGenreAsync cannot be null or empty.");
                }

                if (moviePage < 1)
                {
                    _logger.LogWarning($"Invalid movie page value {moviePage} in {nameof(GetMoviesByGenreAsync)}.");
                    throw new ArgumentOutOfRangeException(nameof(moviePage), "Movie page cannot be less than 1");
                }

                _logger.LogInformation($"Searching movies with genre containing '{genre}' on page {moviePage}.");

                var movieQuery = _dbContext.Movies
                    .AsNoTracking()
                    .Include(m => m.Genre)
                    .Where(m => m.Genre.Name != null && EF.Functions.Like(m.Genre.Name, $"%{genre}%"));

                var totalCount = await movieQuery.CountAsync();

                var moviesByGenre = await movieQuery
                      .OrderBy(m => m.Id)
                      .Skip((moviePage - 1) * PageSize)
                      .Take(PageSize)
                      .ToListAsync();

                if (!moviesByGenre.Any())
                {
                    _logger.LogWarning($"No movies found by genre {genre}");
                    return (Movies: new List<Movie>(), Count: totalCount);
                }

                _logger.LogInformation($"Successfully retrieved {moviesByGenre.Count} movies for genre '{genre}' on page {moviePage}.");
                return (Movies: moviesByGenre, Count: totalCount);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving movies by genre {genre}.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a list of all genres from the database.
        /// Logs a warning if no genres are found and returns an empty list.
        /// Logs an error and rethrows the exception if an error occurs during the retrieval process.
        /// </summary>
        /// <returns>A list of genres from the database, or an empty list if no genres are found.</returns>
        /// <exception cref="Exception">Rethrows any exception that occurs during the retrieval process.</exception>
        public async Task<List<Genre>> GetAllGenresAsync()
        {
            try
            {
                var genres = await _dbContext.Genres
                .AsNoTracking()
                .ToListAsync();

                if (!genres.Any())
                {
                    _logger.LogWarning("No genres found in the database.");
                    return new List<Genre>();
                }

                return genres;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving genres from database.");
                throw;
            }
        }

        /// <summary>
        /// Updates the favorite status of a specified movie for a user based on their chat ID.
        /// If the user does not exist in the database, a new user is created. 
        /// If the movie is not in the user's favorites and `isFavorite` is true, the movie is added to the user's favorites. 
        /// If the movie is already in the user's favorites and `isFavorite` is false, the movie is removed from the user's favorites.
        /// Logs the operation details, including whether a new user was created, and if the movie was found in the database.
        /// Throws an exception if the movie ID is invalid or the movie is not found.
        /// </summary>
        /// <param name="chatId">The chat ID of the user.</param>
        /// <param name="movieId">The ID of the movie to be added or removed from favorites.</param>
        /// <param name="isFavorite">A boolean indicating whether to add or remove the movie from the user's favorites.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the movie ID is less than 1.</exception>
        /// <exception cref="ArgumentException">Thrown when the movie is not found in the database.</exception>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateIsFavoriteAsync(long chatId, int movieId, bool isFavorite)
        {
            _logger.LogInformation($"Starting UpdateIsFavoriteAsync for ChatId: {chatId} and MovieId: {movieId} with IsFavorite: {isFavorite}.");

            if (movieId < 1)
            {
                _logger.LogWarning($"Invalid movie Id value {movieId} in {nameof(UpdateIsFavoriteAsync)}.");
                throw new ArgumentOutOfRangeException(nameof(movieId), "Movie Id cannot be less than 1");
            }

            _logger.LogInformation($"Searching for user with ChatId: '{chatId}'.");
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.ChatId == chatId);

            if (user == null)
            {
                _logger.LogInformation($"User with ChatId: '{chatId}' not found. Creating a new user.");
                user = new User { ChatId = chatId };
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                _logger.LogInformation($"User with ChatId: '{chatId}' found. UserId: {user.Id}");
            }

            _logger.LogInformation($"Searching for movie with MovieId: {movieId}.");
            var movie = await _dbContext.Movies.FindAsync(movieId);

            if (movie == null)
            {
                _logger.LogWarning($"Movie with Id: {movieId} not found.");
                throw new ArgumentException("Movie not found.");
            }

            if (isFavorite)
            {
                _logger.LogInformation($"Adding movie with MovieId: {movieId} to favorites for UserId: {user.Id}.");
                await AddFavoriteMovieAsync(user, movieId);
            }
            else
            {
                _logger.LogInformation($"Removing movie with MovieId: {movieId} from favorites for UserId: {user.Id}.");
                await RemoveFavoriteMovieAsync(user, movieId);
            }

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation($"Successfully updated favorite status for MovieId: {movieId} and UserId: {user.Id}.");
        }

        /// <summary>
        /// Adds a specified movie to the user's list of favorite movies if it's not already present.
        /// Checks if the movie is already in the user's favorites, and if not, adds it to the database.
        /// Logs the operation details, including whether the movie was already in the user's favorites.
        /// </summary>
        /// <param name="user">The user for whom the movie is to be added to the favorites list.</param>
        /// <param name="movieId">The ID of the movie to be added to the user's favorites.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task AddFavoriteMovieAsync(User user, int movieId)
        {
            _logger.LogInformation($"Attempting to add MovieId: {movieId} to UserId: {user.Id}'s favorite movies.");

            var userFavoriteMovie = await _dbContext.UserFavoriteMovies
                .FirstOrDefaultAsync(ufm => ufm.UserId == user.Id && ufm.MovieId == movieId);

            if (userFavoriteMovie == null)
            {
                _logger.LogInformation($"MovieId: {movieId} is not currently in UserId: {user.Id}'s favorites. Adding now.");
                userFavoriteMovie = new UserFavoriteMovie { UserId = user.Id, MovieId = movieId };
                _dbContext.UserFavoriteMovies.Add(userFavoriteMovie);
            }
            else
            {
                _logger.LogInformation($"MovieId: {movieId} is already in UserId: {user.Id}'s favorites. No action taken.");
            }
        }

        /// <summary>
        /// Removes a specified movie from the user's list of favorite movies if it exists.
        /// Checks if the movie is present in the user's favorites, and if found, removes it from the database.
        /// Logs the operation details, including whether the movie was found in the user's favorites.
        /// </summary>
        /// <param name="user">The user from whose favorites list the movie is to be removed.</param>
        /// <param name="movieId">The ID of the movie to be removed from the user's favorites.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task RemoveFavoriteMovieAsync(User user, int movieId)
        {
            _logger.LogInformation($"Attempting to remove MovieId: {movieId} from UserId: {user.Id}'s favorite movies.");

            var userFavoriteMovie = await _dbContext.UserFavoriteMovies
                .FirstOrDefaultAsync(ufm => ufm.UserId == user.Id && ufm.MovieId == movieId);

            if (userFavoriteMovie != null)
            {
                _logger.LogInformation($"MovieId: {movieId} found in UserId: {user.Id}'s favorites. Removing now.");
                _dbContext.UserFavoriteMovies.Remove(userFavoriteMovie);
            }
            else
            {
                _logger.LogInformation($"MovieId: {movieId} was not found in UserId: {user.Id}'s favorites. No action taken.");
            }
        }

        /// <summary>
        /// Retrieves a paginated list of favorite movies for a user specified by their chat ID.
        /// </summary>
        /// <param name="chatId">The chat ID of the user whose favorite movies are to be retrieved.</param>
        /// <param name="moviePage">The page number for pagination, starting from 1. Defaults to 1.</param>
        /// <returns>A tuple containing a list of favorite movies and the total count of favorite movies for the user.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the moviePage is less than 1.</exception>
        /// <exception cref="ArgumentException">Thrown when the user with the specified chat ID is not found.</exception>
        public async Task<(List<Movie> Movies, int Count)> GetListOfFavoriteMoviesAsync(long chatId, int moviePage = 1)
        {
            _logger.LogInformation($"Starting GetListOfFavoriteMoviesAsync for ChatId: {chatId} and moviePage: {moviePage}.");

            // Check for invalid page value
            if (moviePage < 1)
            {
                _logger.LogWarning($"Invalid movie page value {moviePage} provided in {nameof(GetListOfFavoriteMoviesAsync)}.");
                throw new ArgumentOutOfRangeException(nameof(moviePage), "Movie page cannot be less than 1");
            }

            // Retrieve the user by chatId
            var user = await _dbContext.Users
                .AsNoTracking()
                .Include(fm => fm.FavoriteMovies)
                .FirstOrDefaultAsync(u => u.ChatId == chatId);

            if (user == null)
            {
                _logger.LogWarning($"User with ChatId: {chatId} not found.");
                throw new ArgumentException("User not found");
            }

            _logger.LogInformation($"User with ChatId: {chatId} found. Retrieving favorite movies.");

            var query = _dbContext.UserFavoriteMovies
                .AsNoTracking()
                .Where(ufm => ufm.UserId == user.Id)
                .Select(ufm => ufm.Movie);

            // Count total favorite movies
            var totalCount = await query.CountAsync();
            _logger.LogInformation($"Total count of favorite movies for UserId: {user.Id} is {totalCount}.");

            // Retrieve the paginated list of favorite movies
            var favoriteMovies = await query
                .OrderBy(m => m.Id)
                .Skip((moviePage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            _logger.LogInformation($"Retrieved {favoriteMovies.Count} favorite movies for UserId: {user.Id} on page: {moviePage}.");

            return (Movies: favoriteMovies, Count: totalCount);
        }
    }
}