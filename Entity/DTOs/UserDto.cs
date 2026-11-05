using Core.Entities;
using Core.Entities.Concrete;
using System;

namespace Entity.DTOs
{
    /// <summary>
    /// API'den dönen kullanıcı bilgisi. PasswordHash ve PasswordSalt asla dışarı verilmez.
    /// </summary>
    public class UserDto : IDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public static UserDto FromEntity(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserName = user.UserName,
                Status = user.Status,
                CreatedDate = user.CreatedDate,
                UpdatedDate = user.UpdatedDate
            };
        }
    }

    /// <summary>
    /// Yöneticinin kullanıcı güncellerken değiştirebileceği alanlar. Parola bu DTO ile değiştirilemez.
    /// </summary>
    public class UserUpdateDto : IDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public bool Status { get; set; }
    }
}
