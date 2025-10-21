using Core.Entities;
using Core.Utilites.Results.Pagination;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Core.DataAccess.EntityFramework
{
    public class EfEntityRepositoryBase<TEntity, TContext> : IEntityRepository<TEntity>
        where TEntity : class, IEntity, new()
        where TContext : DbContext, new()
    {
        public void Add(TEntity entity)
        {
            //IDissposible pattern iplementation of C#
            using (TContext context = new TContext())
            {
                // Audit: CreatedDate ve Status otomatik ayarla
                entity.CreatedDate = DateTime.Now;
                entity.Status = true; // Yeni kayıtlar default olarak aktif

                var addedEntity = context.Entry(entity);
                addedEntity.State = EntityState.Added;
                context.SaveChanges();
            }
        }

        public void Delete(TEntity entity)
        {
            using (TContext context = new TContext())
            {
                var deletedEntity = context.Entry(entity);
                deletedEntity.State = EntityState.Deleted;
                context.SaveChanges();
            }
        }

        public TEntity Get(Expression<Func<TEntity, bool>> filter)
        {
            using (TContext context = new TContext())
            {
                return context.Set<TEntity>().SingleOrDefault(filter);
            }
        }

        public List<TEntity> GetAll(Expression<Func<TEntity, bool>> filter = null)
        {
            using (TContext context = new TContext())
            {
                return filter == null
                    ? context.Set<TEntity>().ToList()
                    : context.Set<TEntity>().Where(filter).ToList();
            }
        }

        public void Update(TEntity entity)
        {
            using (TContext context = new TContext())
            {
                // Audit: UpdatedDate otomatik ayarla
                entity.UpdatedDate = DateTime.Now;

                var updatedEntity = context.Entry(entity);
                updatedEntity.State = EntityState.Modified;
                
                // Audit Fix: CreatedDate'i güncelleme dışında bırak
                // CreatedDate sadece Add işleminde set edilir, Update'te değişmez
                updatedEntity.Property(nameof(IEntity.CreatedDate)).IsModified = false;
                
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Sayfalanmış, sıralanmış ve filtrelenmiş sonuç döner
        /// </summary>
        public PagedResult<TEntity> GetPaged(
            PaginationParams paginationParams,
            Expression<Func<TEntity, bool>> filter = null)
        {
            using (TContext context = new TContext())
            {
                IQueryable<TEntity> query = context.Set<TEntity>();

                // Filtre varsa uygula
                if (filter != null)
                {
                    query = query.Where(filter);
                }

                // Pagination helper ile sonucu döndür
                return query.ToPagedResult(paginationParams);
            }
        }

    }
}
