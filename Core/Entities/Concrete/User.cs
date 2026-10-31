using System;

namespace Core.Entities.Concrete
{
    public class User : IEntity
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public bool Status { get; set; }

        /// <summary>
        /// Yönetici tarafından açılan veya parolası sıfırlanan hesaplarda true olur.
        /// Kullanıcı parolasını değiştirene kadar sadece parola değiştirme ucunu kullanabilir.
        /// </summary>
        public bool MustChangePassword { get; set; }

        // Audit Alanları (IEntity'den gelir)
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
