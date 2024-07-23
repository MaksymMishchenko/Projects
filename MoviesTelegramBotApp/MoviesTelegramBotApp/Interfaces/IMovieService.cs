using MoviesTelegramBotApp.Models;

namespace MoviesTelegramBotApp.Interfaces
{
    public interface IMovieService
    {
        Task<int> Count { get; }
        Task<List<Movie>> GetAllMoviesAsync(int moviePage = 1);
        Task<List<Movie>> FindMoviesByTitleAsync(string searchString);       
    }
}
