using OnlineMarket.Models;
using System.Threading.Tasks;

namespace OnlineMarket.DataAccess.Repository.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        Task UpdateAsync(Product item);
    }
}
