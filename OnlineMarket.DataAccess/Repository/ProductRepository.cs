using OnlineMarket.DataAccess.Data;
using OnlineMarket.DataAccess.Repository.IRepository;
using OnlineMarket.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace OnlineMarket.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task UpdateAsync(Product item)
        {
            var model = await _context.Products.FirstOrDefaultAsync(x => x.Id == item.Id);

            if (model != null)
            {
                if(item.ImageUrl != null)
                {
                    model.ImageUrl = item.ImageUrl;
                }

                model.Name = item.Name;
                model.Manufacturer = item.Manufacturer;
                model.Price = item.Price;
                model.ListPrice = item.ListPrice;
                model.Description = item.Description;

                
            }
        }
    }
}
