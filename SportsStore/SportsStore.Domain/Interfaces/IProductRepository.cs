using SportsStore.Domain.Entities;

namespace SportsStore.Domain.Interfaces
{
    public interface IProductRepository
    {
        IQueryable<Product> Products { get; }
    }
}
