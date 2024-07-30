namespace MoviesTelegramBotApp.Models
{
    internal class UserState
    {
        public string? CurrentGenre { get; set; }
        public int CurrentPage { get; set; } = 1;
    }
}
