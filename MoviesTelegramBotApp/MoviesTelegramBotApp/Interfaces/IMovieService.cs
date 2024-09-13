using MoviesTelegramBotApp.Models;

namespace MoviesTelegramBotApp.Interfaces
{
    public interface IMovieService
    {
        Task<(List<Movie> Movies, int Count)> GetAllMoviesAsync(int moviePage = 1);
        Task<(List<Movie> Movies, int Count)> GetMoviesByTitleAsync(string searchString, int moviePage);
        Task<Movie> GetRandomMovieAsync();
        Task<(List<Movie> Movies, int Count)> GetMoviesByGenreAsync(string genre, int moviePage);
        Task<List<Genre>> GetAllGenresAsync();
        Task UpdateIsFavoriteAsync(int movieId, bool isFavorite);
        Task<(List<Movie> Movies, int Count)> GetListOfFavoriteMoviesAsync(int moviePage);
    }
}
