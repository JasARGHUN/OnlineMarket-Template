using OnlineMarket.Models;

namespace OnlineMarket.DataAccess.Repository.IRepository
{
    public interface IOrderDetailsRepository : IRepository<OrderDetails>
    {
        void Update(OrderDetails item);
    }
}
