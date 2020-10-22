using OnlineMarket.Models;

namespace OnlineMarket.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader item);
    }
}
