using OnlineMarket.DataAccess.Data;
using OnlineMarket.DataAccess.Repository.IRepository;
using OnlineMarket.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OnlineMarket.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderHeaderRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(OrderHeader item)
        {
            _context.Update(item);
        }
    }
}
