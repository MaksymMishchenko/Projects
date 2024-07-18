using MoviesTelegramBotApp.Models;

namespace MoviesTelegramBotApp.Interfaces
{
    internal interface IMovieService
    {
        Task<List<Movie>> GetAllMoviesAsync();
        string BuildMoviesResponse(List<Movie> movies);
    }
}
