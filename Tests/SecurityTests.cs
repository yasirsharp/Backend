using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Core.Entities.Concrete;
using Core.Utilities.Security.Hashing;
using Core.Utilities.Security.JWT;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Tests
{
    public class HashingHelperTests
    {
        [Fact]
        public void Pbkdf2_hash_verifies_only_the_same_password()
        {
            HashingHelper.CreatePasswordHash("Parola123", out var hash, out var salt);

            Assert.True(HashingHelper.VerifyPasswordHash("Parola123", hash, salt));
            Assert.False(HashingHelper.VerifyPasswordHash("parola123", hash, salt));
            Assert.False(HashingHelper.NeedsRehash(salt));
        }

        [Fact]
        public void Legacy_hmacsha512_hash_is_verified_and_flagged_for_rehash()
        {
            // Eski format: HMACSHA512 anahtarı (128 bayt) salt olarak saklanıyordu
            using var hmac = new HMACSHA512();
            var salt = hmac.Key;
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes("EskiParola1"));

            Assert.True(HashingHelper.VerifyPasswordHash("EskiParola1", hash, salt));
            Assert.False(HashingHelper.VerifyPasswordHash("Yanlis1", hash, salt));
            Assert.True(HashingHelper.NeedsRehash(salt));
        }
    }

    public class PasswordGeneratorTests
    {
        [Fact]
        public void Generates_random_passwords_without_ambiguous_characters()
        {
            var passwords = Enumerable.Range(0, 50).Select(_ => PasswordGenerator.Generate()).ToList();

            Assert.All(passwords, p => Assert.Equal(12, p.Length));
            Assert.All(passwords, p => Assert.DoesNotContain(p, c => "0O1lI".Contains(c)));
            Assert.Equal(passwords.Count, passwords.Distinct().Count());
        }
    }

    public class JwtHelperTests
    {
        private static JwtHelper CreateHelper()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["TokenOptions:Audience"] = "test",
                    ["TokenOptions:Issuer"] = "test",
                    ["TokenOptions:AccessTokenExpiration"] = "10",
                    ["TokenOptions:SecurityKey"] = new string('k', 64)
                })
                .Build();
            return new JwtHelper(configuration);
        }

        private static JwtSecurityToken Decode(AccessToken token) => new JwtSecurityTokenHandler().ReadJwtToken(token.Token);

        [Fact]
        public void Token_contains_pwd_change_claim_only_when_password_must_be_changed()
        {
            var helper = CreateHelper();
            var roles = new List<OperationClaim> { new() { Id = 3, Name = "ogrenci" } };
            var user = new User { Id = 7, Email = "a@b.c", FirstName = "Ali", LastName = "Veli", MustChangePassword = true };

            var withFlag = Decode(helper.CreateToken(user, roles, new()));
            Assert.Contains(withFlag.Claims, c => c.Type == CustomClaimTypes.PasswordChangeRequired);

            user.MustChangePassword = false;
            var withoutFlag = Decode(helper.CreateToken(user, roles, new()));
            Assert.DoesNotContain(withoutFlag.Claims, c => c.Type == CustomClaimTypes.PasswordChangeRequired);
            Assert.Contains(withoutFlag.Claims, c => c.Value == "ogrenci");
        }
    }
}
