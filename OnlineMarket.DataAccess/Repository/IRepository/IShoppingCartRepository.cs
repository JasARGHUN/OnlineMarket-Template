using OnlineMarket.Models;

namespace OnlineMarket.DataAccess.Repository.IRepository
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
        void Update(ShoppingCart item);
    }
}
