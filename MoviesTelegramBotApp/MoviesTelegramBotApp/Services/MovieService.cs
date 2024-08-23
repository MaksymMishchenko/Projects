using Microsoft.EntityFrameworkCore;
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
        private readonly Random _random;
        public int PageSize = 1;

        public Task<int> CountAsync => _dbContext.Movies.CountAsync();

        public MovieService(ApplicationDbContext dbContext, Random random)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _random = random;
        }

        /// <summary>
        /// Asynchronously retrieves all movies from the database, including their genres.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing a list of <see cref="Movie"/> objects.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no movies are available or if there was an issue retrieving movies from the database.</exception>
        private async Task<List<Movie>> GetAllMoviesAsync()
        {
            var movies = await _dbContext.Movies
                .Include(m => m.Genre)
                .ToListAsync();            

            if (movies == null || !movies.Any())
            {
                throw new InvalidOperationException($"No movies available or couldn't retrieve movies from the database.");
            }

            return movies;
        }

        /// <summary>
        /// Asyncronously retrieves a paginated list of all movies from the database, including their genres.
        /// </summary>
        /// <param name="moviePage">The page number to retrieve. Defaults to 1.</param>
        /// <returns>A list of movies from the specified page.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the database connection fails or no movies are found.</exception>
        public async Task<List<Movie>> GetAllMoviesAsync(int moviePage = 1)
        {
            var movies = await _dbContext.Movies.
                Include(m => m.Genre)
                .OrderBy(m => m.Id)
                .Skip((moviePage - 1) * PageSize)
                .Take(PageSize)
            .ToListAsync();            

            if (movies == null || !movies.Any())
            {
                throw new InvalidOperationException($"No movies available or couldn't retrieve movies from the database.");
            }

            return movies;
        }

        // <summary>
        /// Asyncronously retrieves a paginated list of movies whose titles contain the specified search string.
        /// </summary>
        /// <param name="searchString">The string to search for within movie titles.</param>
        /// <param name="moviePage">The page number to retrieve. Defaults to 1.</param>
        /// <returns>A list of movies that contain the search string in their titles.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when no movies are found matching the search string.</exception>
        public async Task<List<Movie>> GetMoviesByTitleAsync(string searchString, int moviePage = 1)
        {
            var foundMovie = await _dbContext.Movies
                .Include(g => g.Genre)
                .Where(m => m.Title!.Contains(searchString))
                .OrderBy(m => m.Id)
                .Skip((moviePage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            if (foundMovie == null || !foundMovie.Any())
            {
                throw new KeyNotFoundException($"Couldn't find the movie by'{searchString}'.");
            }

            return foundMovie;
        }

        /// <summary>
        /// Asynchronously gets the count of movies that contain the specified title substring.
        /// </summary>
        /// <param name="searchString">The substring to search for in movie titles.</param>
        /// <returns>The count of movies whose titles contain the specified substring.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the search string is null or empty.</exception>
        /// <remarks>
        /// This method checks if the search string is null or empty and throws an ArgumentNullException if it is. 
        /// It then queries the database to count the number of movies whose titles contain the search string.
        /// </remarks>
        public async Task<int> GetMoviesByTitleCountAsync(string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
            {
                throw new ArgumentNullException(nameof(searchString), "Search string is null or empty.");
            }

            return await _dbContext.Movies
            .Where(m => m.Title!.Contains(searchString))
            .CountAsync();
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
                var movies = await GetAllMoviesAsync();
                int randomIndex = _random.Next(1, movies.Count);

                return movies[randomIndex];
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves a paginated list of movies by genre and the total count of movies in that genre.
        /// </summary>
        /// <param name="genre">The genre to search for.</param>
        /// <param name="moviePage">The page number for pagination. Default is 1.</param>
        /// <returns>A task representing the asynchronous operation, containing a tuple with a list of movies and the total count of movies in that genre.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the genre string is null or empty.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no movies are found for the specified genre.</exception>
        public async Task<(List<Movie> Movies, int Count)> GetMoviesByGenreAsync(string genre, int moviePage = 1)
        {
            if (string.IsNullOrEmpty(genre))
            {
                throw new ArgumentNullException(nameof(genre), "Search genre string is null or empty.");
            }

            var movieQuery = _dbContext.Movies.Include(m => m.Genre).Where(m => m.Genre.Name == genre);

            var totalCount = await movieQuery.CountAsync();

            var moviesByGenre = await _dbContext.Movies
                  .Include(m => m.Genre)
                  .Where(m => m.Genre.Name == genre)
                  .OrderBy(m => m.Id)
                  .Skip((moviePage - 1) * PageSize)
                  .Take(PageSize)
                  .ToListAsync();

            if (moviesByGenre == null || !moviesByGenre.Any())
            {
                throw new KeyNotFoundException($"Сouldn't find movies by genre: '{genre}'.");
            }

            return (Movies: moviesByGenre, Count: totalCount);
        }

        /// <summary>
        /// Asynchronously retrieves a distinct list of all genres from the movies in the database.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing a list of unique <see cref="Genre"/> objects.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no genres are found in the database.</exception>
        public async Task<List<Genre>> GetAllGenresAsync()
        {

            var movieGenres = await _dbContext.Movies
            .Include(m => m.Genre)
            .Select(m => m.Genre)
            .Distinct()
            .ToListAsync() ?? new List<Genre>();

            if (movieGenres == null || !movieGenres.Any())
            {
                throw new InvalidOperationException("No genres found");
            }

            return movieGenres;
        }

        /// <summary>
        /// Asynchronously updates the `IsFavorite` property of a movie in the database.
        /// If the movie with the specified ID does not exist, an `ArgumentNullException` is thrown.
        /// </summary>
        /// <param name="movieId">The ID of the movie to update.</param>
        /// <param name="isFavorite">The new favorite status to set for the movie.</param>
        /// <exception cref="ArgumentNullException">Thrown when the movie with the specified ID is not found in the database.</exception>      
        public async Task UpdateIsFavoriteAsync(int movieId, bool isFavorite)
        {
            var foundMovie = await _dbContext.Movies.FindAsync(movieId);

            //ArgumentNullException.ThrowIfNull(foundMovie);
            if (foundMovie == null)
            {
                throw new ArgumentNullException("An error occurred during finding the movie in database. Throw from MovieService in line 204");
            }

            foundMovie.IsFavorite = isFavorite;
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

            //ArgumentNullException.ThrowIfNull(favoriteMoviesList);
            if (favoriteMoviesList == null)
            {
                throw new ArgumentNullException("An error occurred during finding the movies in database. Throw from MovieService in line 231");
            }

            return (Movies: favoriteMoviesList, Count: count);
        }
    }
}