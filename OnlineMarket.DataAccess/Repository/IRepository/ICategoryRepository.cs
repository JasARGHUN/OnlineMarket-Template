using OnlineMarket.Models;
using System.Threading.Tasks;

namespace OnlineMarket.DataAccess.Repository.IRepository
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task UpdateAsync(Category item);
    }
}
