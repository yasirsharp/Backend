using Core.Entities;
using Core.Utilites.Results.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Core.DataAccess
{
    public interface IEntityRepository<T> where T : class, IEntity, new()
    {
        void Add(T entity);
        List<T> GetAll(Expression<Func<T, bool>> filter = null);
        T Get(Expression<Func<T, bool>> filter = null);
        void Update(T entity);
        void Delete(T entity);

        /// <summary>
        /// Sayfalanmış, sıralanmış ve filtrelenmiş sonuç döner
        /// </summary>
        PagedResult<T> GetPaged(
            PaginationParams paginationParams,
            Expression<Func<T, bool>> filter = null);
    }
}
