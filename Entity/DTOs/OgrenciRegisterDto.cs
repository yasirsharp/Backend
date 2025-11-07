using Core.Entities;

namespace Entity.DTOs
{
    /// <summary>
    /// Öğrenci kayıt DTO'su - TEST amaçlı kullanılır
    /// </summary>
    public class OgrenciRegisterDto : IDto
    {
        // User bilgileri
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        
        // Öğrenci bilgileri
        public required string OgrenciNo { get; set; }
        public int BolumId { get; set; }
        public int? Sinif { get; set; }
    }
}
