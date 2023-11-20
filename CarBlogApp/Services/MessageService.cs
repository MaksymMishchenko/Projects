using CarBlogApp.Interfaces;
using CarBlogApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CarBlogApp.Services
{
    public class MessageService : IMessageService, IDisposable
    {
        private readonly DatabaseContext _dbContext;

        public MessageService(DatabaseContext db)
        {
            _dbContext = db;
        }

        /// <summary>
        /// Method is tasked with asynchronously retrieving inbox messages
        /// </summary>
        /// <returns>
        /// A collection of messages
        /// </returns>
        public async Task<IEnumerable<ContactForm>> GetInboxMessagesAsync()
        {
            if (_dbContext != null)
            {
                return await _dbContext.InboxMessages.ToListAsync();
            }

            return Enumerable.Empty<ContactForm>();
        }

        // <summary>
        /// Adds a new message to the inbox asynchronously.
        /// </summary>
        /// <param name="message">The ContactForm representing the message to be added.</param>
        /// <returns>
        /// True if the message was successfully added; otherwise, false.
        /// </returns>
        public async Task<bool> AddMessageToInbox(ContactForm message)
        {
            if (_dbContext != null)
            {
                _dbContext.InboxMessages.Add(message);
                await _dbContext.SaveChangesAsync();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes an inbox message asynchronously by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the inbox message to be removed.</param>
        /// <returns>
        /// True if the inbox message was successfully removed; otherwise, false.
        /// </returns>
        public async Task<bool> RemoveInboxMessageAsync(int? id)
        {
            var foundMessage = await _dbContext.InboxMessages.FindAsync(id);

            if (foundMessage != null && _dbContext != null)
            {
                _dbContext.InboxMessages.Remove(foundMessage);
                await _dbContext.SaveChangesAsync();

                return true;
            }

            return false;
        }
        public void Dispose()
        {
            _dbContext?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
