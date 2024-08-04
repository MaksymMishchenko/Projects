using MoviesTelegramBotApp.Models;

namespace MoviesTelegramBotApp.Interfaces
{
    public interface IMovieService
    {
        Task<int> CountAsync { get; }
        Task<List<Movie>> GetAllMoviesAsync(int moviePage = 1);
        Task<List<Movie>> GetMoviesByTitleAsync(string searchString, int moviePage);
        Task<int> GetMoviesByTitleCountAsync(string searchString);
        Task<Movie> GetRandomMovieAsync();
        Task<(List<Movie> Movies, int Count)> GetMoviesByGenreAsync(string genre, int moviePage);
        Task<List<Genre>> GetAllGenresAsync();
        Task UpdateIsFavoriteAsync(int movieId, bool isFavorite);
        Task<List<Movie>> GetListOfFavoriteMoviesAsync();
    }
}
