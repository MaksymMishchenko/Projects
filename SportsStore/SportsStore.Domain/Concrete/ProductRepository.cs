using SportsStore.Domain.Entities;
using SportsStore.Domain.Interfaces;

namespace SportsStore.Domain.Concrete
{
    public class ProductRepository : IProductRepository
    {
        private readonly DatabaseContext _context;
        public ProductRepository(DatabaseContext context)
        {
            _context = context;
        }
        public IQueryable<Product> Products => _context.Products;
    }
}
