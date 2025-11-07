using Core.Entities;

namespace Entity.DTOs
{
    /// <summary>
    /// Akademik Personel kayıt DTO'su - TEST amaçlı kullanılır
    /// </summary>
    public class AkademikPersonelRegisterDto : IDto
    {
        // User bilgileri
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        
        // Akademik Personel bilgileri
        public int UnvanId { get; set; }
        public string? Telefon { get; set; }
        public string? Adres { get; set; }
        
        // İlk bölüm ataması (opsiyonel)
        public int? BolumId { get; set; }
    }
}
