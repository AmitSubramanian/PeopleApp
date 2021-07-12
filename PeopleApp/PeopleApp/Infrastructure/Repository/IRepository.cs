using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeopleApp.Infrastructure.Repository
{
    public interface IRepository<T> where T : class
    {
        Task<List<T>> GetAll();
        Task<T> Get(Guid id);
        Task<Result> Add(T record);
        Task<Result> Update(T record);
        Task<Result> Delete(Guid id);
    }
}
