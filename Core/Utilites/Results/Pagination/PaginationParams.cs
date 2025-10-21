using System;

namespace Core.Utilites.Results.Pagination
{
    /// <summary>
    /// Sayfalama parametreleri
    /// </summary>
    public class PaginationParams
    {
        private const int MaxPageSize = 100;
        private int _pageSize = 10;

        /// <summary>
        /// Sayfa numarası (1'den başlar)
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Sayfa başına kayıt sayısı
        /// </summary>
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }

        /// <summary>
        /// Sıralama alanı (örn: "Ad", "CreatedDate")
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Sıralama yönü (asc, desc)
        /// </summary>
        public string SortOrder { get; set; } = "asc";

        /// <summary>
        /// Arama terimi (genel arama için)
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Artan sıralama mı?
        /// </summary>
        public bool IsAscending => SortOrder?.ToLower() == "asc";
    }
}
