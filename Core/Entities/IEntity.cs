using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    /// <summary>
    /// Tüm entity'lerin implement etmesi gereken temel interface
    /// Audit alanları (CreatedDate, UpdatedDate) otomatik yönetilir
    /// Status alanı (true: Aktif, false: Pasif) soft delete için kullanılır
    /// </summary>
    public interface IEntity
    {
        DateTime CreatedDate { get; set; }
        DateTime? UpdatedDate { get; set; }
        bool Status { get; set; } // true: Aktif, false: Pasif
    }
}
