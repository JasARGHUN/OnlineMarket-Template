using OnlineMarket.Models;
using System.Threading.Tasks;

namespace OnlineMarket.DataAccess.Repository.IRepository
{
    public interface ICompanyRepository : IRepository<Company>
    {
        void Update(Company item);
    }
}
