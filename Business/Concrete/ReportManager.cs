using Business.Abstract;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entity.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Business.Concrete
{
    /// <summary>
    /// Rapor yönetim servisi
    /// QuestPDF ile profesyonel PDF raporları
    /// </summary>
    public class ReportManager : IReportService
    {
        private readonly ISinavDetayDal _sinavDetayDal;
        private readonly IDerslikDal _derslikDal;

        /// <summary>duzce.edu.tr laciverti (frontend teması ile aynı)</summary>
        private const string BrandColor = "#003366";
        private static readonly System.Globalization.CultureInfo Tr = new("tr-TR");

        public ReportManager(ISinavDetayDal sinavDetayDal, IDerslikDal derslikDal)
        {
            _sinavDetayDal = sinavDetayDal;
            _derslikDal = derslikDal;
        }

        /// <summary>
        /// Derslik ID'lerini derslik adlarına çevir
        /// </summary>
        private string GetDersliklerText(List<DerslikGozetmenDTO> derslikler)
        {
            if (derslikler == null || !derslikler.Any())
                return "-";

            var derslikIds = derslikler.Select(d => d.DerslikId).Distinct().ToList();
            var derslikAdlari = _derslikDal.GetAll(d => derslikIds.Contains(d.Id))
                .Select(d => d.Ad)
                .ToList();

            return derslikAdlari.Any() ? string.Join(", ", derslikAdlari) : "-";
        }

        /// <summary>
        /// Resmi sınav takvimi raporu oluştur
        /// QuestPDF ile profesyonel PDF render
        /// </summary>
        public IDataResult<byte[]> GenerateOfficialSinavReport(DateTime startDate, DateTime endDate, int[] bolumIds)
        {
            try
            {
                // QuestPDF lisansını ayarla (Community kullanımı için)
                QuestPDF.Settings.License = LicenseType.Community;

                // Sınav verilerini al
                var sinavlar = _sinavDetayDal.GetSinavDetailsByDateRange(startDate, endDate);

                // Bölüm filtresi uygula
                if (bolumIds != null && bolumIds.Length > 0)
                {
                    sinavlar = sinavlar.Where(s => bolumIds.Contains(s.BolumId)).ToList();
                }

                if (sinavlar.Count == 0)
                {
                    return new ErrorDataResult<byte[]>("Belirtilen tarih aralığında sınav bulunamadı.");
                }

                // PDF oluştur
                var pdfBytes = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape()); // YATAY
                        page.Margin(1.5f, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                        // Header
                        page.Header().Element(c => ComposeHeader(c, "SINAV TAKVİMİ"));

                        // Content
                        page.Content().Element(c => ComposeContent(c, sinavlar, startDate, endDate));

                        // Footer
                        page.Footer().Element(ComposeFooter);
                    });
                }).GeneratePdf();

                return new SuccessDataResult<byte[]>(pdfBytes, "Rapor başarıyla oluşturuldu.");
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<byte[]>(ExceptionLogger.Log(ex, "Rapor oluşturulurken hata."));
            }
        }

        /// <summary>
        /// PDF header bileşeni
        /// </summary>
        private static void ComposeHeader(IContainer container, string title)
        {
            container.Column(column =>
            {
                column.Item().AlignCenter().Text("DÜZCE ÜNİVERSİTESİ")
                    .FontSize(18)
                    .Bold()
                    .FontColor(BrandColor);

                column.Item().PaddingTop(5).AlignCenter().Text(title)
                    .FontSize(14)
                    .SemiBold()
                    .FontColor(BrandColor);

                column.Item().PaddingTop(10).LineHorizontal(2).LineColor(BrandColor);
            });
        }

        /// <summary>
        /// PDF content bileşeni
        /// </summary>
        private void ComposeContent(IContainer container, List<SinavDetayDTO> sinavlar, DateTime startDate, DateTime endDate)
        {
            container.Column(column =>
            {
                // Tarih aralığı bilgisi
                column.Item().PaddingVertical(10).AlignCenter()
                    .Text($"Tarih Aralığı: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}")
                    .FontSize(11)
                    .FontColor(Colors.Grey.Darken2);

                // Sınav tablosu
                column.Item().Table(table =>
                {
                    // Tablo sütunları: 8 sütun
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);  // Bölüm Adı
                        columns.RelativeColumn(4);  // Ders Adı
                        columns.RelativeColumn(3);  // Öğretim Görevlisi
                        columns.RelativeColumn(2);  // Sınav Tarihi
                        columns.RelativeColumn(2);  // Sınav Günü
                        columns.RelativeColumn(1.5f);  // Başlangıç Saati
                        columns.RelativeColumn(1.5f);  // Bitiş Saati
                        columns.RelativeColumn(3);  // Derslikler
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Background(BrandColor).Text("Bölüm Adı").FontColor(Colors.White).Bold();
                        header.Cell().Element(CellStyle).Background(BrandColor).Text("Ders Adı").FontColor(Colors.White).Bold();
                        header.Cell().Element(CellStyle).Background(BrandColor).Text("Öğretim Görevlisi").FontColor(Colors.White).Bold();
                        header.Cell().Element(CellStyle).Background(BrandColor).Text("Sınav Tarihi").FontColor(Colors.White).Bold();
                        header.Cell().Element(CellStyle).Background(BrandColor).Text("Sınav Günü").FontColor(Colors.White).Bold();
                        header.Cell().Element(CellStyle).Background(BrandColor).Text("Başlangıç").FontColor(Colors.White).Bold();
                        header.Cell().Element(CellStyle).Background(BrandColor).Text("Bitiş").FontColor(Colors.White).Bold();
                        header.Cell().Element(CellStyle).Background(BrandColor).Text("Derslikler").FontColor(Colors.White).Bold();

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.BorderBottom(1).BorderColor(Colors.White).PaddingVertical(5).PaddingHorizontal(3);
                        }
                    });

                    // Satırlar
                    int rowIndex = 0;
                    foreach (var sinav in sinavlar.OrderBy(s => s.SinavTarihi))
                    {
                        var backgroundColor = rowIndex % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                        rowIndex++;

                        // Sınav günü hesapla
                        string sinavGunu = sinav.SinavTarihi.ToString("dddd", new System.Globalization.CultureInfo("tr-TR"));
                        
                        // Derslikler listesi oluştur
                        string derslikler = GetDersliklerText(sinav.Derslikler);

                        table.Cell().Element(container => CellStyle(container, backgroundColor)).Text(sinav.BolumAd);
                        table.Cell().Element(container => CellStyle(container, backgroundColor)).Text(sinav.DersAd);
                        table.Cell().Element(container => CellStyle(container, backgroundColor)).Text($"{sinav.Unvan} {sinav.AkademikPersonelAd}");
                        table.Cell().Element(container => CellStyle(container, backgroundColor)).Text(sinav.SinavTarihi.ToString("dd.MM.yyyy"));
                        table.Cell().Element(container => CellStyle(container, backgroundColor)).Text(sinavGunu);
                        table.Cell().Element(container => CellStyle(container, backgroundColor)).Text(sinav.SinavBaslangicSaati.ToString("HH:mm"));
                        table.Cell().Element(container => CellStyle(container, backgroundColor)).Text(sinav.SinavBitisSaati.ToString("HH:mm"));
                        table.Cell().Element(container => CellStyle(container, backgroundColor)).Text(derslikler).FontSize(8);
                    }

                    static IContainer CellStyle(IContainer container, string backgroundColor)
                    {
                        return container
                            .Border(1)
                            .BorderColor(Colors.Grey.Lighten2)
                            .Background(backgroundColor)
                            .PaddingVertical(5)
                            .PaddingHorizontal(3);
                    }
                });

                // İstatistik bilgisi
                column.Item().PaddingTop(15).AlignRight()
                    .Text($"Toplam Sınav Sayısı: {sinavlar.Count}")
                    .FontSize(11)
                    .Bold()
                    .FontColor(BrandColor);

                // İmza alanı
                column.Item().PaddingTop(40).AlignRight().Column(signatureColumn =>
                {
                    signatureColumn.Item().Text("____________________");
                    signatureColumn.Item().PaddingTop(5).Text("Yetkili İmza")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1);
                });
            });
        }

        /// <summary>
        /// PDF footer bileşeni
        /// </summary>
        private static void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(x =>
            {
                x.DefaultTextStyle(style => style.FontSize(8).FontColor(Colors.Grey.Medium));
                x.Span($"Oluşturulma Tarihi: {DateTime.Now:dd.MM.yyyy HH:mm}");
                x.Span(" | ");
                x.Span("Düzce Üniversitesi Sınav Takvimi Sistemi");
            });
        }

        /// <summary>
        /// Gözetmen görevlendirme raporu: her gözetmenin görevleri tarih sırasıyla (gözetmene göre gruplu).
        /// </summary>
        public IDataResult<byte[]> GenerateGozetmenAssignmentReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                var sinavlar = _sinavDetayDal.GetSinavDetailsByDateRange(startDate, endDate);
                var derslikAdlari = _derslikDal.GetAll().ToDictionary(d => d.Id, d => d.Ad);

                var gorevler = sinavlar
                    .SelectMany(s => (s.Derslikler ?? new List<DerslikGozetmenDTO>())
                        .Where(d => d.GozetmenId.HasValue)
                        .Select(d => new
                        {
                            Sinav = s,
                            Gozetmen = s.Gozetmenler?.FirstOrDefault(g => g.Id == d.GozetmenId),
                            Derslik = derslikAdlari.GetValueOrDefault(d.DerslikId, "-")
                        }))
                    .Where(x => x.Gozetmen != null)
                    .OrderBy(x => x.Gozetmen!.Ad, StringComparer.Create(Tr, false))
                    .ThenBy(x => x.Sinav.SinavTarihi)
                    .ThenBy(x => x.Sinav.SinavBaslangicSaati)
                    .Select(x => new[]
                    {
                        $"{x.Gozetmen!.Unvan} {x.Gozetmen.Ad}",
                        x.Sinav.SinavTarihi.ToString("dd.MM.yyyy"),
                        x.Sinav.SinavTarihi.ToString("dddd", Tr),
                        $"{x.Sinav.SinavBaslangicSaati:HH\\:mm} - {x.Sinav.SinavBitisSaati:HH\\:mm}",
                        x.Sinav.DersAd,
                        x.Sinav.BolumAd,
                        x.Derslik
                    })
                    .ToList();

                if (gorevler.Count == 0)
                    return new ErrorDataResult<byte[]>("Belirtilen tarih aralığında gözetmen ataması bulunamadı.");

                var gozetmenSayisi = gorevler.Select(r => r[0]).Distinct().Count();
                var pdf = TableReport("GÖZETMEN GÖREVLENDİRME LİSTESİ", startDate, endDate,
                    new[] { "Gözetmen", "Tarih", "Gün", "Saat", "Ders", "Bölüm", "Derslik" },
                    new[] { 3f, 1.6f, 1.6f, 1.8f, 3.5f, 3f, 1.8f },
                    gorevler,
                    $"Toplam {gorevler.Count} görev, {gozetmenSayisi} gözetmen");
                return new SuccessDataResult<byte[]>(pdf, "Rapor başarıyla oluşturuldu.");
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<byte[]>(ExceptionLogger.Log(ex, "Rapor oluşturulurken hata."));
            }
        }

        /// <summary>
        /// Derslik kullanım raporu: her derslikte yapılan sınavlar (dersliğe göre gruplu) ve toplam kullanım süresi.
        /// </summary>
        public IDataResult<byte[]> GenerateDerslikUsageReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                var sinavlar = _sinavDetayDal.GetSinavDetailsByDateRange(startDate, endDate);
                var derslikler = _derslikDal.GetAll().ToDictionary(d => d.Id);

                var kullanimlar = sinavlar
                    .SelectMany(s => (s.Derslikler ?? new List<DerslikGozetmenDTO>()).Select(d => new { Sinav = s, d.DerslikId }))
                    .Where(x => derslikler.ContainsKey(x.DerslikId))
                    .GroupBy(x => x.DerslikId)
                    .OrderBy(g => derslikler[g.Key].Ad, StringComparer.Create(Tr, false))
                    .ToList();

                if (kullanimlar.Count == 0)
                    return new ErrorDataResult<byte[]>("Belirtilen tarih aralığında derslik kullanımı bulunamadı.");

                var satirlar = new List<string[]>();
                foreach (var grup in kullanimlar)
                {
                    var derslik = derslikler[grup.Key];
                    var toplamDakika = grup.Sum(x => (x.Sinav.SinavBitisSaati - x.Sinav.SinavBaslangicSaati).TotalMinutes);
                    var ozet = $"{derslik.Ad} ({derslik.Kapasite} kişilik) · {grup.Count()} sınav, {toplamDakika / 60:0.#} saat";
                    foreach (var x in grup.OrderBy(x => x.Sinav.SinavTarihi).ThenBy(x => x.Sinav.SinavBaslangicSaati))
                    {
                        satirlar.Add(new[]
                        {
                            ozet,
                            x.Sinav.SinavTarihi.ToString("dd.MM.yyyy"),
                            x.Sinav.SinavTarihi.ToString("dddd", Tr),
                            $"{x.Sinav.SinavBaslangicSaati:HH\\:mm} - {x.Sinav.SinavBitisSaati:HH\\:mm}",
                            x.Sinav.DersAd,
                            x.Sinav.BolumAd
                        });
                    }
                }

                var pdf = TableReport("DERSLİK KULLANIM RAPORU", startDate, endDate,
                    new[] { "Derslik", "Tarih", "Gün", "Saat", "Ders", "Bölüm" },
                    new[] { 4f, 1.6f, 1.6f, 1.8f, 3.5f, 3f },
                    satirlar,
                    $"{kullanimlar.Count} derslik, {satirlar.Count} kullanım");
                return new SuccessDataResult<byte[]>(pdf, "Rapor başarıyla oluşturuldu.");
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<byte[]>(ExceptionLogger.Log(ex, "Rapor oluşturulurken hata."));
            }
        }

        /// <summary>
        /// Yatay A4 tablo raporu. İlk sütun gruplama sütunudur: aynı değer art arda geldiğinde sadece ilk satırda yazılır.
        /// </summary>
        private static byte[] TableReport(string title, DateTime startDate, DateTime endDate,
            string[] headers, float[] widths, List<string[]> rows, string summary)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));
                    page.Header().Element(c => ComposeHeader(c, title));
                    page.Content().Column(column =>
                    {
                        column.Item().PaddingVertical(10).AlignCenter()
                            .Text($"Tarih Aralığı: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}")
                            .FontSize(11).FontColor(Colors.Grey.Darken2);

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                foreach (var w in widths)
                                    columns.RelativeColumn(w);
                            });

                            table.Header(header =>
                            {
                                foreach (var h in headers)
                                    header.Cell().Background(BrandColor).PaddingVertical(5).PaddingHorizontal(3)
                                        .Text(h).FontColor(Colors.White).Bold();
                            });

                            string? oncekiGrup = null;
                            var grupIndex = -1;
                            foreach (var row in rows)
                            {
                                var yeniGrup = row[0] != oncekiGrup;
                                if (yeniGrup) grupIndex++;
                                oncekiGrup = row[0];
                                var bg = grupIndex % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                                for (var i = 0; i < row.Length; i++)
                                {
                                    var text = i == 0 && !yeniGrup ? "" : row[i];
                                    var cell = table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Background(bg)
                                        .PaddingVertical(4).PaddingHorizontal(3).Text(text);
                                    if (i == 0) cell.SemiBold();
                                }
                            }
                        });

                        column.Item().PaddingTop(15).AlignRight().Text(summary).FontSize(11).Bold().FontColor(BrandColor);
                    });
                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf();
        }
    }
}
