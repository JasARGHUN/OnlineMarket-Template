using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OnlineMarket.DataAccess.Data;
using OnlineMarket.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Linq;

namespace OnlineMarket.DataAccess.Repository
{
    public class SP_Call : ISP_Call
    {
        private readonly ApplicationDbContext _context;
        private static string ConnectionString = "";

        public SP_Call(ApplicationDbContext context)
        {
            _context = context;
            ConnectionString = _context.Database.GetDbConnection().ConnectionString;
        }

        public void Dispose()
        {
           _context.Dispose();
        }

        public async Task Execute(string procedureName, DynamicParameters parameters = null)
        {
            using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            {
                await sqlConnection.OpenAsync();
                await sqlConnection.ExecuteAsync(procedureName, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<IEnumerable<T>> List<T>(string procedureName, DynamicParameters parameters = null)
        {
            using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            {
                await sqlConnection.OpenAsync();
                return await sqlConnection.QueryAsync<T>(procedureName, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<Tuple<IEnumerable<T1>, IEnumerable<T2>>> List<T1, T2>(string procedureName, DynamicParameters parameters = null)
        {
            using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            {
                await sqlConnection.OpenAsync();
                var result = SqlMapper.QueryMultiple(sqlConnection, procedureName, parameters, commandType: System.Data.CommandType.StoredProcedure);
                var itemOne = result.Read<T1>().ToList();
                var itemTwo = result.Read<T2>().ToList();

                if(itemOne != null && itemTwo != null)
                {
                    return new Tuple<IEnumerable<T1>, IEnumerable<T2>>(itemOne, itemTwo);
                }
            }

            return new Tuple<IEnumerable<T1>, IEnumerable<T2>>(new List<T1>(), new List<T2>());
        }

        public async Task<T> OneRecord<T>(string procedureName, DynamicParameters parameters = null)
        {
            using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            {
                await sqlConnection.OpenAsync();
                var value = await sqlConnection.QueryAsync<T>(procedureName, parameters, commandType: CommandType.StoredProcedure);

                return (T)Convert.ChangeType(value.FirstOrDefault(), typeof(T));
            }
        }

        public async Task<T> Single<T>(string procedureName, DynamicParameters parameters = null)
        {
            using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            {
                await sqlConnection.OpenAsync();
                return (T)Convert.ChangeType(await sqlConnection.ExecuteScalarAsync<T>(procedureName, parameters, commandType: CommandType.StoredProcedure),
                    typeof(T));
            }
        }
    }
}
