using Business.Constants;
using Core.Extensions;
using DataAccess.Concrete.EntityFramework;

namespace API.Security
{
    /// <summary>
    /// Bölüm izolasyonu kuralları (saf, test edilebilir). Yönetici her yerde sınırsızdır; bölüm görevlisi yalnızca
    /// kendi bölümlerinin kayıtlarını görür ve değiştirir. Bölümsüz (ortak) kayıtları yalnızca yönetici değiştirir.
    /// </summary>
    public class BolumScopeRules
    {
        public bool IsGlobal { get; }
        /// <summary>Bölüm görevlisi ve yönetici değil: bölüm süzgeci uygulanır</summary>
        public bool IsScoped { get; }
        public IReadOnlySet<int> BolumIds { get; }

        public BolumScopeRules(bool isGlobal, bool isBolumGorevlisi, IEnumerable<int> bolumIds)
        {
            IsGlobal = isGlobal;
            IsScoped = !isGlobal && isBolumGorevlisi;
            BolumIds = bolumIds.ToHashSet();
        }

        public bool CanAccess(int bolumId) => !IsScoped || BolumIds.Contains(bolumId);

        /// <summary>Bir kaydın görülebilmesi: bölümlerinden biri kendi bölümü ya da kayıt ortak (bölümsüz)</summary>
        public bool CanSee(IReadOnlyCollection<int> recordBolumIds) =>
            !IsScoped || recordBolumIds.Count == 0 || recordBolumIds.Any(BolumIds.Contains);

        /// <summary>
        /// Bir kaydın değiştirilebilmesi: ortak (bölümsüz) kayıt yalnızca yönetici; diğerlerinde kaydın bütün bölümleri
        /// kendi bölümleri olmalı (iki bölümün dersini bir bölümün görevlisi değiştiremez).
        /// </summary>
        public bool CanManage(IReadOnlyCollection<int> recordBolumIds) =>
            !IsScoped || (recordBolumIds.Count > 0 && recordBolumIds.All(BolumIds.Contains));
    }

    /// <summary>
    /// İsteği yapan kullanıcının bölüm kapsamı. Görevlinin bölümleri her istekte veritabanından okunur: görevlilikten
    /// alınan kişi, elindeki token süresi dolmasa da hemen yetkisini kaybeder. İstek başına bir kez sorgulanır.
    /// </summary>
    public class BolumScope
    {
        public const string OwnBolumOnly = "Sadece kendi bölümünüzün kayıtlarını görebilir ve değiştirebilirsiniz.";
        public const string SharedAdminOnly = "Ortak veya birden çok bölüme ait kayıtları yalnızca yönetici değiştirebilir.";

        private readonly IHttpContextAccessor _http;
        private BolumScopeRules? _rules;
        private int? _personelId;
        private bool _personelLoaded;

        public BolumScope(IHttpContextAccessor http)
        {
            _http = http;
        }

        private System.Security.Claims.ClaimsPrincipal? User => _http.HttpContext?.User;

        public BolumScopeRules Rules => _rules ??= Load();

        public bool IsScoped => Rules.IsScoped;

        private BolumScopeRules Load()
        {
            var user = User;
            var isGlobal = user?.IsInAnyRole(Roles.Yonetici) == true;
            var isGorevli = user?.IsInRole(Roles.BolumGorevlisi) == true;
            var userId = user?.GetUserId();
            var ids = new List<int>();
            if (!isGlobal && isGorevli && userId != null)
            {
                using var context = new DuzceUniversiteContext();
                ids = (from bap in context.BolumAkademikPersoneller
                       join ap in context.AkademikPersonel on bap.AkademikPersonelId equals ap.Id
                       where ap.UserId == userId.Value
                       select bap.BolumId).Distinct().ToList();
            }
            return new BolumScopeRules(isGlobal, isGorevli, ids);
        }

        /// <summary>Kullanıcının akademik personel kaydı (dersin hocası / gözetmen istisnası için)</summary>
        public int? PersonelId
        {
            get
            {
                if (_personelLoaded) return _personelId;
                _personelLoaded = true;
                var userId = User?.GetUserId();
                if (userId == null) return null;
                using var context = new DuzceUniversiteContext();
                _personelId = context.AkademikPersonel.Where(p => p.UserId == userId.Value).Select(p => (int?)p.Id).FirstOrDefault();
                return _personelId;
            }
        }

        public bool CanAccess(int bolumId) => Rules.CanAccess(bolumId);
        public bool CanSee(IReadOnlyCollection<int> bolumIds) => Rules.CanSee(bolumIds);
        public bool CanManage(IReadOnlyCollection<int> bolumIds) => Rules.CanManage(bolumIds);

        // --- Kayıtların bölümleri (yalnızca kapsamlı kullanıcıda sorgulanır) ---

        public static int? BolumOfDbap(int dbapId)
        {
            using var context = new DuzceUniversiteContext();
            return context.DersBolumAkademikPersonel.Where(x => x.Id == dbapId).Select(x => (int?)x.BolumId).FirstOrDefault();
        }

        public static int? BolumOfSinav(int sinavId)
        {
            using var context = new DuzceUniversiteContext();
            return (from s in context.SinavDetay
                    join d in context.DersBolumAkademikPersonel on s.DersBolumAkademikPersonelId equals d.Id
                    where s.Id == sinavId
                    select (int?)d.BolumId).FirstOrDefault();
        }

        /// <summary>Dersin bölümleri; ders yoksa null</summary>
        public static List<int>? BolumsOfDers(int dersId)
        {
            using var context = new DuzceUniversiteContext();
            if (!context.Ders.Any(d => d.Id == dersId)) return null;
            return context.DersBolum.Where(x => x.DersId == dersId).Select(x => x.BolumId).ToList();
        }

        /// <summary>Dersliğin bölümleri; derslik yoksa null</summary>
        public static List<int>? BolumsOfDerslik(int derslikId)
        {
            using var context = new DuzceUniversiteContext();
            if (!context.Derslik.Any(d => d.Id == derslikId)) return null;
            return context.DerslikBolum.Where(x => x.DerslikId == derslikId).Select(x => x.BolumId).ToList();
        }

        /// <summary>Kullanıcının hocası olduğu ders atamaları</summary>
        public HashSet<int> MyDbapIds()
        {
            var personelId = PersonelId;
            if (personelId == null) return new HashSet<int>();
            using var context = new DuzceUniversiteContext();
            return context.DersBolumAkademikPersonel.Where(x => x.AkademikPersonelId == personelId).Select(x => x.Id).ToHashSet();
        }
    }
}
