using CarBlogApp.Models;

namespace CarBlogApp.Interfaces
{
    public interface IMessageService
    {
        Task<IEnumerable<ContactForm>> GetInboxMessagesAsync();
        Task<bool> AddMessageToInbox(ContactForm message);
        Task<bool> RemoveInboxMessageAsync(int? id);
    }
}
