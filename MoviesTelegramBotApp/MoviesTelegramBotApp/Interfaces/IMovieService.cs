using MoviesTelegramBotApp.Models;

namespace MoviesTelegramBotApp.Interfaces
{
    internal interface IMovieService
    {
        List<Movie> GetMovies();
        string BuildMoviesResponse(List<Movie> movies);
    }
}
