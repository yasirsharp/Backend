using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Core.Utilites.Results.Pagination
{
    /// <summary>
    /// Sayfalama ve sıralama yardımcı metodları
    /// </summary>
    public static class PaginationHelper
    {
        /// <summary>
        /// IQueryable için sayfalama ve sıralama uygular
        /// </summary>
        public static PagedResult<T> ToPagedResult<T>(
            this IQueryable<T> query,
            PaginationParams paginationParams)
        {
            // 1. Toplam kayıt sayısını al
            var totalCount = query.Count();

            // 2. Sıralama uygula
            if (!string.IsNullOrWhiteSpace(paginationParams.SortBy))
            {
                query = ApplySorting(query, paginationParams.SortBy, paginationParams.IsAscending);
            }

            // 3. Sayfalama uygula
            var items = query
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToList();

            // 4. Sonuç döndür
            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = paginationParams.PageNumber,
                PageSize = paginationParams.PageSize,
                SortBy = paginationParams.SortBy,
                SortOrder = paginationParams.SortOrder,
                SearchTerm = paginationParams.SearchTerm
            };
        }

        /// <summary>
        /// List için sayfalama uygular (LINQ to Objects)
        /// </summary>
        public static PagedResult<T> ToPagedResult<T>(
            this List<T> list,
            PaginationParams paginationParams)
        {
            var totalCount = list.Count;

            // Sıralama uygula
            if (!string.IsNullOrWhiteSpace(paginationParams.SortBy))
            {
                list = ApplySorting(list.AsQueryable(), paginationParams.SortBy, paginationParams.IsAscending).ToList();
            }

            // Sayfalama uygula
            var items = list
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToList();

            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = paginationParams.PageNumber,
                PageSize = paginationParams.PageSize,
                SortBy = paginationParams.SortBy,
                SortOrder = paginationParams.SortOrder,
                SearchTerm = paginationParams.SearchTerm
            };
        }

        /// <summary>
        /// Dinamik sıralama uygular
        /// </summary>
        private static IQueryable<T> ApplySorting<T>(
            IQueryable<T> query,
            string sortBy,
            bool isAscending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return query;

            // Property'nin var olup olmadığını kontrol et
            var property = typeof(T).GetProperty(sortBy);
            if (property == null)
                return query; // Property yoksa sıralama yapma

            // Expression tree oluştur
            var parameter = Expression.Parameter(typeof(T), "x");
            var propertyAccess = Expression.Property(parameter, property);
            var lambda = Expression.Lambda(propertyAccess, parameter);

            // OrderBy veya OrderByDescending metodunu çağır
            var methodName = isAscending ? "OrderBy" : "OrderByDescending";
            var resultExpression = Expression.Call(
                typeof(Queryable),
                methodName,
                new Type[] { typeof(T), property.PropertyType },
                query.Expression,
                Expression.Quote(lambda)
            );

            return query.Provider.CreateQuery<T>(resultExpression);
        }
    }
}
