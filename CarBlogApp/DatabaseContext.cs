using CarBlogApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CarBlogApp
{
    public class DatabaseContext : DbContext
    {        
        public DbSet<ContactForm> InboxMessages { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Post> Posts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=CarBlogApp;Trusted_Connection=true");
        }
    }
}
