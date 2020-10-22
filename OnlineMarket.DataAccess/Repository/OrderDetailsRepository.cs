using Microsoft.EntityFrameworkCore;
using OnlineMarket.DataAccess.Data;
using OnlineMarket.DataAccess.Repository.IRepository;
using OnlineMarket.Models;
using System.Threading.Tasks;

namespace OnlineMarket.DataAccess.Repository
{
    public class OrderDetailsRepository : Repository<OrderDetails>, IOrderDetailsRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderDetailsRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(OrderDetails item)
        {
            _context.Update(item);
        }
    }
}
