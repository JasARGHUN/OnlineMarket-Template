using OnlineMarket.DataAccess.Data;
using OnlineMarket.DataAccess.Repository.IRepository;
using System.Threading.Tasks;

namespace OnlineMarket.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Category = new CategoryRepository(_context);
            Product = new ProductRepository(_context);
            Company = new CompanyRepository(_context);
            User = new UserRepository(_context);
            ShoppingCart = new ShoppingCartRepository(_context);
            OrderDetails = new OrderDetailsRepository(_context);
            OrderHeader = new OrderHeaderRepository(_context);
            SP_Call = new SP_Call(_context);
        }

        public ICategoryRepository Category { get; private set; }
        public IProductRepository Product { get; private set; }
        public ICompanyRepository Company { get; private set; }
        public IUserRepository User { get; private set; }
        public ISP_Call SP_Call { get; private set; }
        public IShoppingCartRepository ShoppingCart { get; private set; }
        public IOrderHeaderRepository OrderHeader { get; private set; }
        public IOrderDetailsRepository OrderDetails { get; private set; }

        public void Dispose()
        {
            _context.Dispose();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
