using Microsoft.EntityFrameworkCore;
using OnlineMarket.DataAccess.Data;
using OnlineMarket.DataAccess.Repository.IRepository;
using OnlineMarket.Models;
using System.Threading.Tasks;

namespace OnlineMarket.DataAccess.Repository
{
    public class UserRepository : Repository<ApplicationUser>, IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        //public void Update(ApplicationUser item)
        //{
        //    _context.Update(item);
        //}
    }
}
