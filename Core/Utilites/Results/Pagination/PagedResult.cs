using System;
using System.Collections.Generic;

namespace Core.Utilites.Results.Pagination
{
    /// <summary>
    /// Sayfalanmış sonuç wrapper
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// Mevcut sayfadaki kayıtlar
        /// </summary>
        public List<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// Toplam kayıt sayısı (tüm sayfalardaki)
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Mevcut sayfa numarası
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Sayfa başına kayıt sayısı
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Toplam sayfa sayısı
        /// </summary>
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        /// <summary>
        /// Önceki sayfa var mı?
        /// </summary>
        public bool HasPrevious => PageNumber > 1;

        /// <summary>
        /// Sonraki sayfa var mı?
        /// </summary>
        public bool HasNext => PageNumber < TotalPages;

        /// <summary>
        /// İlk kayıt numarası (görüntüleme için)
        /// </summary>
        public int FirstItemIndex => (PageNumber - 1) * PageSize + 1;

        /// <summary>
        /// Son kayıt numarası (görüntüleme için)
        /// </summary>
        public int LastItemIndex => Math.Min(PageNumber * PageSize, TotalCount);

        /// <summary>
        /// Sıralama bilgisi
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Sıralama yönü
        /// </summary>
        public string? SortOrder { get; set; }

        /// <summary>
        /// Arama terimi
        /// </summary>
        public string? SearchTerm { get; set; }
    }
}
