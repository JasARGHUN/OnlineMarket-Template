using Dapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineMarket.DataAccess.Repository.IRepository
{
    public interface ISP_Call : IDisposable
    {
        Task<T> Single<T>(string procedureName, DynamicParameters parameters = null);

        Task Execute(string procedureName, DynamicParameters parameters = null);

        Task<T> OneRecord<T>(string procedureName, DynamicParameters parameters = null);

        Task<IEnumerable<T>> List<T>(string procedureName, DynamicParameters parameters = null);

        Task<Tuple<IEnumerable<T1>, IEnumerable<T2>>> List<T1, T2>(string procedureName, DynamicParameters parameters = null);
    }
}
