using API.Security;
using Business.Constants;
using Xunit;

namespace Tests
{
    public class ImpersonationTests
    {
        [Fact]
        public void Only_super_admin_can_act_as_active_non_super_users()
        {
            Assert.Null(ImpersonationRules.Check(true, 1, 2, true, new[] { Roles.BolumGorevlisi }));
            Assert.NotNull(ImpersonationRules.Check(false, 1, 2, true, new[] { Roles.Ogrenci }));   // yönetici (Admin) bile olsa
            Assert.NotNull(ImpersonationRules.Check(true, 1, 1, true, new[] { Roles.SuperAdmin })); // kendisi
            Assert.NotNull(ImpersonationRules.Check(true, 1, 2, false, new[] { Roles.Ogrenci }));   // pasif hesap
            Assert.NotNull(ImpersonationRules.Check(true, 1, 2, true, new[] { Roles.SuperAdmin, Roles.AkademikPersonel }));
        }
    }
}
