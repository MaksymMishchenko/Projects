namespace MoviesTelegramBotApp.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Country { get; set; }
        public string? Budget { get; set; }
        public string? ImageUrl { get; set; }
        public string? MovieUrl { get; set; }
        public string? InterestFactsUrl { get; set; }
        public string? BehindTheScene { get; set; }                

        public int GenreId { get; set; }
        public Genre? Genre { get; set; }

        public ICollection<UserFavoriteMovie>? Users { get; set; } = new List<UserFavoriteMovie>();
    }
}
