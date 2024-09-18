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

        public async Task UpdateIsFavoriteAsync(long chatId, int movieId, bool isFavorite)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.ChatId == chatId);

            if (user == null)
            {
                user = new User { ChatId = chatId };
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();
            }

            var movie = await _dbContext.Movies.FindAsync(movieId);

            if (movie == null)
            {
                throw new ArgumentException("Movie not found.");
            }

            if (isFavorite)
            {
                var userFavoriteMovie = await _dbContext.UserFavoriteMovies
                    .FirstOrDefaultAsync(ufm => ufm.UserId == user.Id && ufm.MovieId == movieId);

                if (userFavoriteMovie == null)
                {
                    userFavoriteMovie = new UserFavoriteMovie { UserId = user.Id, MovieId = movieId };
                    _dbContext.UserFavoriteMovies.Add(userFavoriteMovie);
                }
            }
            else
            {
                var userFavoriteMovie = await _dbContext.UserFavoriteMovies
                    .FirstOrDefaultAsync(ufm => ufm.UserId == user.Id && ufm.MovieId == movieId);

                if (userFavoriteMovie != null)
                {
                    _dbContext.UserFavoriteMovies.Remove(userFavoriteMovie);
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Asynchronously retrieves a list of all movies marked as favorites from the database, including their associated genres.
        /// Ensures that the result is not null by throwing an <see cref="ArgumentNullException"/> if the list is null.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result is a list of favorite movies with their associated genres.</returns>
        public async Task<(List<Movie> Movies, int Count)> GetListOfFavoriteMoviesAsync(int moviePage = 1)
        {
            var moviesByTrueFavoriteProperty = _dbContext.Movies
                .Include(m => m.Genre)
                .Where(m => m.IsFavorite == true);

            var count = await moviesByTrueFavoriteProperty.CountAsync();

            var favoriteMoviesList = await _dbContext.Movies
                .Include(m => m.Genre)
                .Where(m => m.IsFavorite == true)
                .OrderBy(m => m.Id)
                .Skip((moviePage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            if (favoriteMoviesList == null)
            {
                throw new ArgumentNullException("An error occurred during finding the movies in database. Throw from MovieService in line 231");
            }

            return (Movies: favoriteMoviesList, Count: count);
        }
    }
}