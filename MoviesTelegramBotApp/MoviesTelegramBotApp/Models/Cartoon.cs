namespace MoviesTelegramBotApp.Models
{
    public class Cartoon
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Budget { get; set; }
        public string? ImageUrl { get; set; }
        public string? CartoonUrl { get; set; }

        public int CartoonGenreId { get; set; }
        public CartoonGenre? Genre { get; set; }
    }
}
