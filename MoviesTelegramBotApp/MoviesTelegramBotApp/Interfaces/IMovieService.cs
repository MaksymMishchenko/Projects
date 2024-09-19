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
        Task UpdateIsFavoriteAsync(long chatId, int movieId, bool isFavorite);
        Task<bool> IsMovieInFavoritesAsync(long chatId, int movieId);
        Task<(List<Movie> Movies, int Count)> GetListOfFavoriteMoviesAsync(long chatId, int moviePage = 1);
    }
}
