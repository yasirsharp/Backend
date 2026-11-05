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
        public void Akademik_personel_sees_only_own_tasks()
        {
            Assert.True(AkademikPersonel.IsScoped);
            Assert.True(AkademikPersonel.OwnTasksOnly);
            Assert.False(AkademikPersonel.CanAccess(3));
            Assert.False(AkademikPersonel.CanSee(new List<int>()));   // ortak tanımları da listelemez
            Assert.False(AkademikPersonel.CanManage(new List<int> { 3 }));
        }

        [Fact]
        public void Gorevli_personel_reads_assigned_bolums_but_never_writes()
        {
            var gp = new BolumScopeRules(false, Array.Empty<int>(), new[] { 2, 3 }, isPersonel: true, isGorevliPersonel: true);
            Assert.False(gp.OwnTasksOnly);
            Assert.True(gp.CanAccess(2));
            Assert.True(gp.CanAccess(3));
            Assert.False(gp.CanAccess(1));
            Assert.False(gp.CanWrite(2));
            Assert.False(gp.CanManage(new List<int> { 2 }));
        }

        [Fact]
        public void Gorevli_personel_without_bolum_sees_nothing()
        {
            var gp = new BolumScopeRules(false, Array.Empty<int>(), Array.Empty<int>(), isPersonel: true, isGorevliPersonel: true);
            Assert.False(gp.OwnTasksOnly);
            Assert.False(gp.CanAccess(1));
        }

        [Fact]
        public void Gorevli_and_gorevli_personel_write_only_where_gorevli()
        {
            // Bölüm 1'in görevlisi, bölüm 2'nin görevli personeli: 2'yi görür ama değiştiremez
            var both = new BolumScopeRules(false, new[] { 1 }, new[] { 2 }, isPersonel: true, isBolumGorevlisi: true, isGorevliPersonel: true);
            Assert.True(both.CanAccess(2));
            Assert.False(both.CanWrite(2));
            Assert.True(both.CanWrite(1));
            Assert.False(both.CanManage(new List<int> { 1, 2 }));
        }
    }
}
