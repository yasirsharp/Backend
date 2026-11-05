using System.Globalization;
using System.Security.Claims;
using API.Security;
using Business.Constants;
using Core.Extensions;
using Core.Utilities.Results;
using DataAccess.Concrete.EntityFramework;
using Entity.Concrete;
using Entity.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Sınav değişiklik talepleri: akademik personel dersini verdiği veya gözetmen olduğu sınav için değişiklik ister;
    /// o bölümün görevlisi veya yönetici kabul eder ya da reddeder. Her adımda ilgililere bildirim gider.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Roles.Personel)]
    public class SinavTalepController : ControllerBase
    {
        private static readonly CultureInfo Tr = new("tr-TR");
        private readonly BolumScope _scope;

        public SinavTalepController(BolumScope scope)
        {
            _scope = scope;
        }

        private string MyName =>
            User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.Email)?.Value ?? "Kullanıcı";

        private bool CanAnswer(int bolumId) => _scope.Rules.IsGlobal || _scope.Rules.ManageBolumIds.Contains(bolumId);

        [HttpPost]
        public IActionResult Create([FromBody] SinavTalepCreateDto dto)
        {
            var userId = User.GetUserId();
            var personelId = _scope.PersonelId;
            if (userId == null || personelId == null)
                return BadRequest(new ErrorResult("Hesabınıza bağlı akademik personel kaydı yok."));
            var neden = (dto.Neden ?? "").Trim();
            if (neden.Length < 5)
                return BadRequest(new ErrorResult("Ne değişmesini istediğinizi ve nedenini yazın."));
            if (neden.Length > 1000)
                neden = neden[..1000];

            using var context = new DuzceUniversiteContext();
            var sinav = (from s in context.SinavDetay
                         join d in context.DersBolumAkademikPersonel on s.DersBolumAkademikPersonelId equals d.Id
                         join ders in context.Ders on d.DersId equals ders.Id
                         where s.Id == dto.SinavDetayId
                         select new { s.Id, s.SinavTarihi, s.SinavBaslangicSaati, s.SinavBitisSaati, d.BolumId, d.AkademikPersonelId, ders.Kod, DersAd = ders.Ad })
                .FirstOrDefault();
            if (sinav == null)
                return NotFound(new ErrorResult("Sınav bulunamadı."));

            var gozetmen = context.SinavDerslik.Any(x => x.SinavDetayId == sinav.Id && x.GozetmenId == personelId);
            var hoca = sinav.AkademikPersonelId == personelId;
            if (!hoca && !gozetmen)
                return StatusCode(StatusCodes.Status403Forbidden,
                    new ErrorResult("Yalnızca dersini verdiğiniz veya gözetmen olduğunuz sınavlar için değişiklik isteyebilirsiniz."));
            if (context.SinavDegisiklikTalepleri.Any(x => x.SinavDetayId == sinav.Id && x.TalepEdenUserId == userId && x.Durum == SinavTalepDurumlari.Bekliyor))
                return BadRequest(new ErrorResult("Bu sınav için yanıt bekleyen bir talebiniz zaten var."));

            TimeOnly? Saat(string? v) => TimeOnly.TryParse(v, out var t) ? t : null;
            var ozet = $"{sinav.Kod} {sinav.DersAd} · {sinav.SinavTarihi.ToString("d MMMM yyyy dddd", Tr)} {sinav.SinavBaslangicSaati:HH\\:mm}-{sinav.SinavBitisSaati:HH\\:mm}";
            var talep = new SinavDegisiklikTalebi
            {
                SinavDetayId = sinav.Id,
                BolumId = sinav.BolumId,
                SinavOzeti = ozet.Length > 300 ? ozet[..300] : ozet,
                TalepEdenUserId = userId.Value,
                TalepEdenAd = MyName,
                Gorev = hoca ? "ogretim-elemani" : "gozetmen",
                Neden = neden,
                OnerilenTarih = dto.OnerilenTarih?.Date,
                OnerilenBaslangic = Saat(dto.OnerilenBaslangic),
                OnerilenBitis = Saat(dto.OnerilenBitis),
                CreatedDate = DateTime.Now,
            };
            context.SinavDegisiklikTalepleri.Add(talep);
            context.SaveChanges();

            // Bölümün görevlileri ve yöneticiler
            var gorevliler = (from bap in context.BolumAkademikPersoneller
                              join ap in context.AkademikPersonel on bap.AkademikPersonelId equals ap.Id
                              where bap.BolumId == sinav.BolumId
                              select ap.UserId).ToList();
            var yoneticiler = (from uoc in context.UserOperationClaims
                               join oc in context.OperationClaims on uoc.OperationClaimId equals oc.Id
                               join u in context.Users on uoc.UserId equals u.Id
                               where (oc.Name == Roles.Admin || oc.Name == Roles.SuperAdmin) && u.Status
                               select uoc.UserId).ToList();
            foreach (var alici in gorevliler.Concat(yoneticiler).Where(id => id != userId).Distinct())
                context.Notifications.Add(Bildirim(alici, "Sınav değişiklik talebi", $"{talep.TalepEdenAd}: {ozet}. {neden}", "warning", talep.Id));
            context.SaveChanges();

            return Ok(new SuccessDataResult<int>(talep.Id, "Talebiniz bölüm görevlisine iletildi."));
        }

        /// <summary>Yönetici tümünü, görevli kendi bölümlerininkini, herkes kendi açtıklarını görür</summary>
        [HttpGet]
        public IActionResult GetList([FromQuery] string? durum = null)
        {
            var userId = User.GetUserId();
            using var context = new DuzceUniversiteContext();
            var manage = _scope.Rules.ManageBolumIds.ToList();
            var isGlobal = _scope.Rules.IsGlobal;
            var query = context.SinavDegisiklikTalepleri
                .Where(x => isGlobal || manage.Contains(x.BolumId) || x.TalepEdenUserId == userId);
            if (!string.IsNullOrEmpty(durum))
                query = query.Where(x => x.Durum == durum);
            var sinavIds = context.SinavDetay.Select(s => s.Id);
            var list = (from t in query
                        join b in context.Bolum on t.BolumId equals b.Id into bj
                        from b in bj.DefaultIfEmpty()
                        orderby t.Durum == SinavTalepDurumlari.Bekliyor descending, t.Id descending
                        select new SinavTalepDto
                        {
                            Id = t.Id,
                            SinavDetayId = t.SinavDetayId,
                            SinavVar = sinavIds.Contains(t.SinavDetayId),
                            SinavTarihi = context.SinavDetay.Where(s => s.Id == t.SinavDetayId).Select(s => (DateTime?)s.SinavTarihi).FirstOrDefault(),
                            BolumId = t.BolumId,
                            BolumAd = b != null ? b.Ad : "",
                            SinavOzeti = t.SinavOzeti,
                            TalepEdenAd = t.TalepEdenAd,
                            Gorev = t.Gorev,
                            Neden = t.Neden,
                            OnerilenTarih = t.OnerilenTarih,
                            OnerilenBaslangic = t.OnerilenBaslangic,
                            OnerilenBitis = t.OnerilenBitis,
                            Durum = t.Durum,
                            YanitNotu = t.YanitNotu,
                            YanitlayanAd = t.YanitlayanAd,
                            YanitTarihi = t.YanitTarihi,
                            CreatedDate = t.CreatedDate,
                            Benim = t.TalepEdenUserId == userId,
                            Yanitlayabilir = isGlobal || manage.Contains(t.BolumId),
                        }).Take(500).ToList();
            return Ok(new SuccessDataResult<List<SinavTalepDto>>(list, $"{list.Count} talep bulundu."));
        }

        /// <summary>Menü rozeti: yanıtlayabileceği bekleyen talepler</summary>
        [HttpGet("bekleyen-sayisi")]
        public IActionResult GetPendingCount()
        {
            using var context = new DuzceUniversiteContext();
            var manage = _scope.Rules.ManageBolumIds.ToList();
            var isGlobal = _scope.Rules.IsGlobal;
            var count = context.SinavDegisiklikTalepleri.Count(x =>
                x.Durum == SinavTalepDurumlari.Bekliyor && (isGlobal || manage.Contains(x.BolumId)));
            return Ok(new SuccessDataResult<int>(count));
        }

        [HttpPut("{id:int}/yanit")]
        public IActionResult Answer(int id, [FromBody] SinavTalepYanitDto dto)
        {
            if (dto.Durum != SinavTalepDurumlari.Kabul && dto.Durum != SinavTalepDurumlari.Red)
                return BadRequest(new ErrorResult("Durum kabul veya red olmalı."));
            var not = (dto.Not ?? "").Trim();
            if (dto.Durum == SinavTalepDurumlari.Red && not.Length == 0)
                return BadRequest(new ErrorResult("Reddederken kısa bir gerekçe yazın."));

            using var context = new DuzceUniversiteContext();
            var talep = context.SinavDegisiklikTalepleri.FirstOrDefault(x => x.Id == id);
            if (talep == null)
                return NotFound(new ErrorResult("Talep bulunamadı."));
            if (!CanAnswer(talep.BolumId))
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResult(BolumScope.OwnBolumOnly));
            if (talep.Durum != SinavTalepDurumlari.Bekliyor)
                return BadRequest(new ErrorResult("Bu talep zaten yanıtlandı."));

            talep.Durum = dto.Durum;
            talep.YanitNotu = not.Length == 0 ? null : not.Length > 1000 ? not[..1000] : not;
            talep.YanitlayanUserId = User.GetUserId();
            talep.YanitlayanAd = MyName;
            talep.YanitTarihi = DateTime.Now;
            talep.UpdatedDate = DateTime.Now;
            var sonuc = dto.Durum == SinavTalepDurumlari.Kabul ? "kabul edildi" : "reddedildi";
            context.Notifications.Add(Bildirim(talep.TalepEdenUserId, $"Değişiklik talebiniz {sonuc}",
                $"{talep.SinavOzeti}{(talep.YanitNotu != null ? $". Not: {talep.YanitNotu}" : "")}",
                dto.Durum == SinavTalepDurumlari.Kabul ? "success" : "error", talep.Id));
            context.SaveChanges();
            return Ok(new SuccessResult($"Talep {sonuc}; isteyen kişiye bildirim gönderildi."));
        }

        private static Notification Bildirim(int userId, string baslik, string mesaj, string tip, int talepId) => new()
        {
            UserId = userId,
            Title = baslik,
            Message = mesaj.Length > 2000 ? mesaj[..2000] : mesaj,
            Type = tip,
            IsRead = false,
            ActionUrl = "/talepler",
            RelatedEntityType = "SinavDegisiklikTalebi",
            RelatedEntityId = talepId,
            CreatedDate = DateTime.Now,
            Status = true,
        };
    }
}
