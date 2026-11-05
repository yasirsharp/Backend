using Core.Entities;
using System.Collections.Generic;

namespace Entity.DTOs
{
    /// <summary>
    /// Yöneticinin açtığı hesabın giriş bilgileri. Geçici parola yalnızca bu yanıtta bir kez gösterilir;
    /// kullanıcı ilk girişte parolasını değiştirmek zorundadır.
    /// </summary>
    public class CreatedAccountDto : IDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string TemporaryPassword { get; set; } = "";
    }

    /// <summary>Toplu içe aktarmada bir satırın sonucu. Row, gönderilen listedeki 1'den başlayan sıra numarasıdır.</summary>
    public class BulkRowResultDto : IDto
    {
        public int Row { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public CreatedAccountDto? Account { get; set; }
    }

    public class BulkResultDto : IDto
    {
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<BulkRowResultDto> Rows { get; set; } = new();
    }

    /// <summary>Yöneticinin tek tek veya toplu eklediği öğrenci. E-posta boşsa {OgrenciNo}@ogr.duzce.edu.tr kullanılır.</summary>
    public class OgrenciCreateDto : IDto
    {
        public string OgrenciNo { get; set; } = "";
        public string Ad { get; set; } = "";
        public string Soyad { get; set; } = "";
        public string? Email { get; set; }
        public int BolumId { get; set; }
        public int? Sinif { get; set; }
    }

    public class OgrenciUpdateDto : IDto
    {
        public int Id { get; set; }
        public string OgrenciNo { get; set; } = "";
        public string Ad { get; set; } = "";
        public string Soyad { get; set; } = "";
        public int BolumId { get; set; }
        public int? Sinif { get; set; }
        public bool Status { get; set; } = true;
    }

    /// <summary>Toplu akademik personel kaydı. Ad, unvan hariç tam addır (ör. "Ayşe Yılmaz").</summary>
    public class AkademikPersonelCreateDto : IDto
    {
        public string Ad { get; set; } = "";
        public string Unvan { get; set; } = "";
        public string? Email { get; set; }
    }

    public class ChangePasswordDto : IDto
    {
        public string CurrentPassword { get; set; } = "";
        public string NewPassword { get; set; } = "";
    }

    /// <summary>Kullanıcı yönetimi listesi: kullanıcı bilgileri ve rol adları tek sorguda.</summary>
    public class UserWithRolesDto : UserDto
    {
        public bool MustChangePassword { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
