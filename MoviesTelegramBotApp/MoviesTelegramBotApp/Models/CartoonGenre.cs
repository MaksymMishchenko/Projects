namespace MoviesTelegramBotApp.Models
{
    public class CartoonGenre
    {
        public int Id { get; set; }
        public string? Genre { get; set; }

        public ICollection<Cartoon>? Cartoons { get; set; }
    }
}
