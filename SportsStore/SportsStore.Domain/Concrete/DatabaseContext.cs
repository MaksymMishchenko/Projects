using Microsoft.EntityFrameworkCore;
using SportsStore.Domain.Entities;

namespace SportsStore.Domain.Concrete
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
         : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
    }
}
