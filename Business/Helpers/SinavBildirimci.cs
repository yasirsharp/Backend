using Core.Utilities.Results;
using DataAccess.Abstract;
using Entity.Concrete;
using Entity.DTOs;
using System.Globalization;

namespace Business.Helpers
{
    /// <summary>
    /// Sınav eklenince, değişince veya silinince dersin öğretim elemanına ve gözetmenlere bildirim oluşturur.
    /// Bildirim hatası sınav işlemini bozmaz; sadece loglanır.
    /// </summary>
    public class SinavBildirimci
    {
        private static readonly CultureInfo Tr = new("tr-TR");
        private readonly INotificationDal _notificationDal;

        public SinavBildirimci(INotificationDal notificationDal)
        {
            _notificationDal = notificationDal;
        }

        public void Eklendi(SinavBildirimBilgisiDTO? sinav)
        {
            if (sinav == null) return;
            Gonder(sinav.Gorevliler, sinav, g => (g.Gorev == "gozetmen" ? "Gözetmen görevi" : "Sınavınız planlandı",
                $"{Ozet(sinav)}{GorevMetni(g)}", "info"));
        }

        public void Guncellendi(SinavBildirimBilgisiDTO? once, SinavBildirimBilgisiDTO? sonra)
        {
            if (sonra == null) return;
            Gonder(sonra.Gorevliler, sonra, g => ("Sınav güncellendi", $"{Ozet(sonra)}{GorevMetni(g)}", "warning"));

            // Önceki halinde görevli olup artık olmayanlar
            if (once == null) return;
            var kalanlar = sonra.Gorevliler.Select(g => g.UserId).ToHashSet();
            var cikanlar = once.Gorevliler.Where(g => !kalanlar.Contains(g.UserId)).ToList();
            Gonder(cikanlar, sonra, g => ("Sınav görevi kaldırıldı",
                $"{sonra.DersKod} {sonra.DersAd} sınavındaki göreviniz kaldırıldı.", "warning"));
        }

        public void Silindi(SinavBildirimBilgisiDTO? once)
        {
            if (once == null) return;
            Gonder(once.Gorevliler, null, g => ("Sınav iptal edildi", $"{Ozet(once)} iptal edildi.", "error"));
        }

        private static string Ozet(SinavBildirimBilgisiDTO s) =>
            $"{s.DersKod} {s.DersAd} sınavı: {s.SinavTarihi.ToString("d MMMM yyyy dddd", Tr)}, " +
            $"{s.BaslangicSaati:HH\\:mm}-{s.BitisSaati:HH\\:mm}" +
            (s.Derslikler.Count > 0 ? $", {string.Join(", ", s.Derslikler)}" : "");

        private static string GorevMetni(SinavGorevliDTO g) =>
            g.Gorev == "gozetmen" ? $". Gözetmen olarak görevlendirildiniz ({g.Derslik})." : ".";

        private void Gonder(IEnumerable<SinavGorevliDTO> gorevliler, SinavBildirimBilgisiDTO? sinav,
            Func<SinavGorevliDTO, (string Baslik, string Mesaj, string Tip)> icerik)
        {
            // Öğretim elemanı kendi sınavında gözetmen olabilir, bir kişi birden fazla derslikte gözetmen olabilir:
            // kişi başına tek bildirim gider.
            foreach (var g in gorevliler.GroupBy(x => x.UserId).Select(x => x.First()))
            {
                try
                {
                    var (baslik, mesaj, tip) = icerik(g);
                    _notificationDal.Add(new Notification
                    {
                        UserId = g.UserId,
                        Title = baslik,
                        Message = mesaj,
                        Type = tip,
                        IsRead = false,
                        ActionUrl = "/sinav-takvimi",
                        RelatedEntityType = sinav == null ? null : "SinavDetay",
                        RelatedEntityId = sinav?.SinavId,
                        CreatedDate = DateTime.Now,
                        Status = true
                    });
                }
                catch (Exception ex)
                {
                    ExceptionLogger.Log(ex, "Sınav bildirimi oluşturulamadı.");
                }
            }
        }
    }
}
