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
        /// <summary>Okuma süzgeci uygulanır (yönetici dışında herkes)</summary>
        public bool IsScoped { get; }
        /// <summary>Yalnızca kendi görevleri: bölümü olmayan akademik personel (dersini verdiği ve gözetmen olduğu sınavlar)</summary>
        public bool OwnTasksOnly { get; }
        /// <summary>Görebileceği bölümler: bölüm görevlisi + görevli personel bölümleri</summary>
        public IReadOnlySet<int> BolumIds { get; }
        /// <summary>Değiştirebileceği bölümler: yalnızca bölüm görevlisi olduğu bölümler</summary>
        public IReadOnlySet<int> ManageBolumIds { get; }

        public BolumScopeRules(bool isGlobal, bool isBolumGorevlisi, IEnumerable<int> bolumIds)
            : this(isGlobal, isBolumGorevlisi ? bolumIds : Array.Empty<int>(), Array.Empty<int>(), isPersonel: true, isBolumGorevlisi) { }

        /// <param name="gorevliBolumIds">Bölüm görevlisi olduğu bölümler</param>
        /// <param name="okumaBolumIds">Görevli personel olarak atandığı bölümler (salt okunur)</param>
        /// <param name="isPersonel">Personel rolü var mı (öğrenci bu kuralların dışında, kendi kurallarıyla)</param>
        public BolumScopeRules(bool isGlobal, IEnumerable<int> gorevliBolumIds, IEnumerable<int> okumaBolumIds, bool isPersonel,
            bool isBolumGorevlisi = false, bool isGorevliPersonel = false)
        {
            IsGlobal = isGlobal;
            ManageBolumIds = gorevliBolumIds.ToHashSet();
            BolumIds = gorevliBolumIds.Concat(okumaBolumIds).ToHashSet();
            IsScoped = !isGlobal && isPersonel;
            // Bölüm görevlisi ya da görevli personel rolü olan kişi bölüm kapsamındadır (bölümü atanmamış olsa bile)
            OwnTasksOnly = IsScoped && !isBolumGorevlisi && !isGorevliPersonel && BolumIds.Count == 0;
        }

        /// <summary>Okuma: bölümün kayıtlarını görebilir mi</summary>
        public bool CanAccess(int bolumId) => !IsScoped || BolumIds.Contains(bolumId);

        /// <summary>Yazma: bölümün kayıtlarını değiştirebilir mi (yalnız bölüm görevlisi olduğu bölümler)</summary>
        public bool CanWrite(int bolumId) => !IsScoped || ManageBolumIds.Contains(bolumId);

        /// <summary>Bir kaydın görülebilmesi: bölümlerinden biri görebildiği bölüm ya da kayıt ortak (bölümsüz)</summary>
        public bool CanSee(IReadOnlyCollection<int> recordBolumIds) =>
            !IsScoped || (!OwnTasksOnly && (recordBolumIds.Count == 0 || recordBolumIds.Any(BolumIds.Contains)));

        /// <summary>
        /// Bir kaydın değiştirilebilmesi: ortak (bölümsüz) kayıt yalnızca yönetici; diğerlerinde kaydın bütün bölümleri
        /// bölüm görevlisi olduğu bölümler olmalı (iki bölümün dersini bir bölümün görevlisi değiştiremez).
        /// </summary>
        public bool CanManage(IReadOnlyCollection<int> recordBolumIds) =>
            !IsScoped || (recordBolumIds.Count > 0 && recordBolumIds.All(ManageBolumIds.Contains));
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
            var isPersonel = user?.IsInAnyRole(Roles.Personel) == true;
            var isGorevli = user?.IsInRole(Roles.BolumGorevlisi) == true;
            var isGp = user?.IsInRole(Roles.GorevliPersonel) == true;
            var userId = user?.GetUserId();
            var gorevli = new List<int>();
            var okuma = new List<int>();
            if (!isGlobal && userId != null && (isGorevli || isGp))
            {
                using var context = new DuzceUniversiteContext();
                if (isGorevli)
                    gorevli = (from bap in context.BolumAkademikPersoneller
                               join ap in context.AkademikPersonel on bap.AkademikPersonelId equals ap.Id
                               where ap.UserId == userId.Value
                               select bap.BolumId).Distinct().ToList();
                if (isGp)
                    okuma = context.KullaniciBolum.Where(x => x.UserId == userId.Value).Select(x => x.BolumId).ToList();
            }
            return new BolumScopeRules(isGlobal, gorevli, okuma, isPersonel, isGorevli, isGp);
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
        public bool CanWrite(int bolumId) => Rules.CanWrite(bolumId);
        public bool OwnTasksOnly => Rules.OwnTasksOnly;
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
