using MoviesTelegramBotApp.Models;

namespace MoviesTelegramBotApp.Interfaces
{
    internal interface ICartoonService
    {
        Task<List<Cartoon>> GetAllCartoonsAsync(int cartoonPage);
        Task<int> Count { get; }
        Task<List<Cartoon>> GetCartoonsByGenre(string genre);
    }
}
