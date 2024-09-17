namespace MoviesTelegramBotApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public long ChatId { get; set; }
        public ICollection<UserFavoriteMovie>? FavoriteMovies { get; set; } = new List<UserFavoriteMovie>();
    }
}
