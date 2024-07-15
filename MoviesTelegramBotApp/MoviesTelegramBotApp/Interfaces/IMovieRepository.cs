using MoviesTelegramBotApp.Models;

namespace MoviesTelegramBotApp.Interfaces
{
    internal interface IMovieRepository
    {
        IQueryable<Movie> GetMovies { get; }
    }
}
