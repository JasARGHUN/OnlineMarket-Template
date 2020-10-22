using OnlineMarket.DataAccess.Data;
using OnlineMarket.DataAccess.Repository.IRepository;
using OnlineMarket.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OnlineMarket.DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task UpdateAsync(Category item)
        {
            var model = await _context.Categories.FirstOrDefaultAsync(x => x.Id == item.Id);

            if(model != null)
            {
                model.Name = item.Name;
            }
        }
    }
}
