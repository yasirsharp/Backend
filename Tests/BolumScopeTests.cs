using API.Security;
using Xunit;

namespace Tests
{
    public class BolumScopeTests
    {
        private static readonly BolumScopeRules Yonetici = new(isGlobal: true, isBolumGorevlisi: true, new[] { 1 });
        private static readonly BolumScopeRules Gorevli = new(isGlobal: false, isBolumGorevlisi: true, new[] { 1, 2 });
        private static readonly BolumScopeRules BolumsuzGorevli = new(isGlobal: false, isBolumGorevlisi: true, Array.Empty<int>());
        private static readonly BolumScopeRules AkademikPersonel = new(isGlobal: false, isBolumGorevlisi: false, Array.Empty<int>());

        [Fact]
        public void Yonetici_is_never_scoped()
        {
            Assert.False(Yonetici.IsScoped);
            Assert.True(Yonetici.CanAccess(99));
            Assert.True(Yonetici.CanManage(new List<int>())); // ortak kayıt
            Assert.True(Yonetici.CanManage(new List<int> { 5, 6 }));
        }

        [Fact]
        public void Gorevli_accesses_only_own_bolums()
        {
            Assert.True(Gorevli.IsScoped);
            Assert.True(Gorevli.CanAccess(1));
            Assert.True(Gorevli.CanAccess(2));
            Assert.False(Gorevli.CanAccess(3));
        }

        [Fact]
        public void Gorevli_sees_own_and_shared_records()
        {
            Assert.True(Gorevli.CanSee(new List<int>()));        // ortak ders/derslik
            Assert.True(Gorevli.CanSee(new List<int> { 2, 3 })); // bölümlerinden biri kendi bölümü
            Assert.False(Gorevli.CanSee(new List<int> { 3 }));
        }

        [Fact]
        public void Gorevli_manages_only_records_entirely_in_own_bolums()
        {
            Assert.True(Gorevli.CanManage(new List<int> { 1 }));
            Assert.True(Gorevli.CanManage(new List<int> { 1, 2 }));
            Assert.False(Gorevli.CanManage(new List<int> { 1, 3 })); // iki bölümlü ders, biri başkasının
            Assert.False(Gorevli.CanManage(new List<int>()));       // ortak kayıt: yalnızca yönetici
        }

        [Fact]
        public void Gorevli_without_bolum_can_do_nothing()
        {
            Assert.True(BolumsuzGorevli.IsScoped);
            Assert.False(BolumsuzGorevli.CanAccess(1));
            Assert.False(BolumsuzGorevli.CanManage(new List<int> { 1 }));
            Assert.True(BolumsuzGorevli.CanSee(new List<int>())); // ortak kayıtları yine görür
        }

        [Fact]
        public void Other_personel_is_not_bolum_scoped()
        {
            // Akademik/görevli personelin yazma yetkisi rollerle (Planlayici) zaten kapalı; okuma süzgeci uygulanmaz
            Assert.False(AkademikPersonel.IsScoped);
            Assert.True(AkademikPersonel.CanAccess(3));
        }
    }
}
