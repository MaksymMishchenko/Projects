using MoviesTelegramBotApp.Interfaces;

namespace MoviesTelegramBotApp.Models
{
    internal class MovieRepository : IMovieRepository
    {
        private ApplicationDbContext _dbContext;
        public MovieRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }              
        public IQueryable<Movie> GetMovies => _dbContext.Movies;
    }
}
