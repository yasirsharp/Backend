using System.Security.Claims;
using API.Middleware;
using Business.Helpers;
using Business.ValidationRules.FluentValidation;
using Core.Utilities.Security.Hashing;
using Core.Utilities.Security.JWT;
using Entity.DTOs;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Tests
{
    public class AccountFactoryTests
    {
        [Fact]
        public void New_user_gets_temporary_password_and_must_change_it()
        {
            var user = AccountFactory.NewUser(" Ali ", "Veli", "ALI@Duzce.edu.tr", "ali", out var temp);

            Assert.True(user.MustChangePassword);
            Assert.True(user.Status);
            Assert.Equal("ali@duzce.edu.tr", user.Email);
            Assert.Equal("Ali", user.FirstName);
            Assert.True(HashingHelper.VerifyPasswordHash(temp, user.PasswordHash, user.PasswordSalt));
        }

        [Theory]
        [InlineData("Ayşe Nur Yılmaz", "Ayşe Nur", "Yılmaz")]
        [InlineData("Mehmet Öz", "Mehmet", "Öz")]
        [InlineData("Platon", "Platon", "Platon")]
        public void Splits_full_name(string full, string first, string last)
        {
            Assert.Equal((first, last), AccountFactory.SplitFullName(full));
        }

        [Fact]
        public void User_names_are_ascii_lowercase()
        {
            Assert.Equal("cagri.ozturk", AccountFactory.ToAsciiLower("Çağrı.Öztürk"));
            Assert.Equal("isik", AccountFactory.ToAsciiLower("IŞIK"));
        }

        [Fact]
        public void Generates_unique_user_name_from_email_or_initials()
        {
            var taken = new HashSet<string> { "zeynep.celik" };

            Assert.Equal("zeynep.celik2", AccountFactory.GenerateUserName("Zeynep Çelik", "zeynep.celik@duzce.edu.tr", taken.Contains));
            Assert.Matches("^mo[0-9]{4}$", AccountFactory.GenerateUserName("Mehmet Öz", null, taken.Contains));
        }
    }

    public class ValidatorTests
    {
        [Fact]
        public void Change_password_requires_strong_and_different_password()
        {
            var validator = new ChangePasswordDtoValidator();

            Assert.False(validator.Validate(new ChangePasswordDto { CurrentPassword = "Eski1234", NewPassword = "kisa" }).IsValid);
            Assert.False(validator.Validate(new ChangePasswordDto { CurrentPassword = "Eski1234", NewPassword = "Eski1234" }).IsValid);
            Assert.True(validator.Validate(new ChangePasswordDto { CurrentPassword = "Eski1234", NewPassword = "Yeni12345" }).IsValid);
        }

        [Fact]
        public void Ogrenci_create_rules()
        {
            var validator = new OgrenciCreateDtoValidator();
            var valid = new OgrenciCreateDto { OgrenciNo = "2026001", Ad = "Ali", Soyad = "Veli", BolumId = 1, Sinif = 2 };

            Assert.True(validator.Validate(valid).IsValid);
            Assert.False(validator.Validate(new OgrenciCreateDto { OgrenciNo = "20 26", Ad = "A", Soyad = "B", BolumId = 1 }).IsValid);
            Assert.False(validator.Validate(new OgrenciCreateDto { OgrenciNo = "1", Ad = "A", Soyad = "B", BolumId = 0 }).IsValid);
            Assert.False(validator.Validate(new OgrenciCreateDto { OgrenciNo = "1", Ad = "A", Soyad = "B", BolumId = 1, Email = "yok" }).IsValid);
        }
    }

    public class PasswordChangeRequiredMiddlewareTests
    {
        private static async Task<(int Status, bool NextCalled)> Run(string path, bool mustChange)
        {
            var nextCalled = false;
            var middleware = new PasswordChangeRequiredMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "1") };
            if (mustChange)
                claims.Add(new Claim(CustomClaimTypes.PasswordChangeRequired, "true"));

            var context = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"))
            };
            context.Request.Path = path;
            context.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(context);
            return (context.Response.StatusCode, nextCalled);
        }

        [Theory]
        [InlineData("/api/Bolum")]
        [InlineData("/api/User/with-roles")]
        public async Task Blocks_other_api_calls_until_password_is_changed(string path)
        {
            var (status, next) = await Run(path, mustChange: true);
            Assert.Equal(StatusCodes.Status403Forbidden, status);
            Assert.False(next);
        }

        [Theory]
        [InlineData("/api/Auth/change-password")]
        [InlineData("/api/user/me")]
        [InlineData("/sinav-takvimi")]
        public async Task Allows_password_change_profile_and_spa(string path)
        {
            var (_, next) = await Run(path, mustChange: true);
            Assert.True(next);
        }

        [Fact]
        public async Task Normal_users_are_not_affected()
        {
            var (_, next) = await Run("/api/Bolum", mustChange: false);
            Assert.True(next);
        }
    }
}
