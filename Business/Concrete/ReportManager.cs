using Business.Abstract;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entity.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
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

        public ReportManager(ISinavDetayDal sinavDetayDal, IDerslikDal derslikDal)
        {
            _sinavDetayDal = sinavDetayDal;
            _derslikDal = derslikDal;
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
                        page.Size(PageSizes.A4);
                        page.Margin(1.5f, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                        // Header
                        page.Header().Element(ComposeHeader);

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
                return new ErrorDataResult<byte[]>($"Rapor oluşturulurken hata: {ex.Message}");
            }
        }

        /// <summary>
        /// PDF header bileşeni
        /// </summary>
        private void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().AlignCenter().Text("DÜZCE ÜNİVERSİTESİ")
                    .FontSize(18)
                    .Bold()
                    .FontColor("#5B21B6");

                column.Item().PaddingTop(5).AlignCenter().Text("SINAV TAKVİMİ")
                    .FontSize(14)
                    .SemiBold()
                    .FontColor("#5B21B6");

                column.Item().PaddingTop(10).LineHorizontal(2).LineColor("#5B21B6");
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
                    // Tablo sütunları
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);  // Tarih
                        columns.RelativeColumn(4);  // Ders Adı
                        columns.RelativeColumn(3);  // Bölüm
                        columns.RelativeColumn(4);  // Öğretim Görevlisi
                        columns.RelativeColumn(2);  // Saat
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Background("#5B21B6").Text("Tarih").FontColor(Colors.White).Bold();
                        header.Cell().Element(CellStyle).Background("#5B21B6").Text("Ders Adı").FontColor(Colors.White).Bold();
                        header.Cell().Element(CellStyle).Background("#5B21B6").Text("Bölüm").FontColor(Colors.White).Bold();
                        header.Cell().Element(CellStyle).Background("#5B21B6").Text("Öğretim Görevlisi").FontColor(Colors.White).Bold();
                        header.Cell().Element(CellStyle).Background("#5B21B6").Text("Saat").FontColor(Colors.White).Bold();

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

                        table.Cell().Element(container => CellStyle(container, backgroundColor)).Text(sinav.SinavTarihi.ToString("dd.MM.yyyy"));
                        table.Cell().Element(container => CellStyle(container, backgroundColor)).Text(sinav.DersAd);
                        table.Cell().Element(container => CellStyle(container, backgroundColor)).Text(sinav.BolumAd);
                        table.Cell().Element(container => CellStyle(container, backgroundColor)).Text($"{sinav.Unvan} {sinav.AkademikPersonelAd}");
                        table.Cell().Element(container => CellStyle(container, backgroundColor)).Text($"{sinav.SinavBaslangicSaati} - {sinav.SinavBitisSaati}");
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
                    .FontColor("#5B21B6");

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
        private void ComposeFooter(IContainer container)
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
        /// Gözetmen atama raporu
        /// TODO: RDLC şablonu ile PDF oluşturulacak
        /// </summary>
        public IDataResult<byte[]> GenerateGozetmenAssignmentReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                var sinavlar = _sinavDetayDal.GetSinavDetailsByDateRange(startDate, endDate);

                // Gözetmen ataması olan sınavları filtrele
                var gozetmenAtamalari = sinavlar
                    .SelectMany(s => s.Gozetmenler.Select(g => new
                    {
                        SinavTarihi = s.SinavTarihi,
                        SinavBaslangic = s.SinavBaslangicSaati,
                        SinavBitis = s.SinavBitisSaati,
                        DersAd = s.DersAd,
                        BolumAd = s.BolumAd,
                        GozetmenAd = g.Ad,
                        GozetmenUnvan = g.Unvan
                    }))
                    .OrderBy(x => x.SinavTarihi)
                    .ThenBy(x => x.SinavBaslangic)
                    .ToList();

                if (gozetmenAtamalari.Count == 0)
                {
                    return new ErrorDataResult<byte[]>("Gözetmen ataması bulunamadı.");
                }

                // TODO: RDLC rapor oluşturma kodu buraya gelecek
                return new ErrorDataResult<byte[]>("RDLC rapor sistemi henüz implement edilmedi.");
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<byte[]>($"Rapor oluşturulurken hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Derslik kullanım raporu
        /// TODO: RDLC şablonu ile PDF oluşturulacak
        /// </summary>
        public IDataResult<byte[]> GenerateDerslikUsageReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                var sinavlar = _sinavDetayDal.GetSinavDetailsByDateRange(startDate, endDate);

                // Derslik kullanım istatistikleri
                var derslikKullanimi = sinavlar
                    .SelectMany(s => s.Derslikler.Select(d => new
                    {
                        DerslikId = d.DerslikId,
                        SinavTarihi = s.SinavTarihi,
                        SinavSaati = $"{s.SinavBaslangicSaati} - {s.SinavBitisSaati}",
                        DersAd = s.DersAd,
                        BolumAd = s.BolumAd
                    }))
                    .GroupBy(x => x.DerslikId)
                    .Select(g => new
                    {
                        DerslikId = g.Key,
                        KullanimSayisi = g.Count(),
                        SinavDetaylari = g.ToList()
                    })
                    .OrderByDescending(x => x.KullanimSayisi)
                    .ToList();

                if (derslikKullanimi.Count == 0)
                {
                    return new ErrorDataResult<byte[]>("Derslik kullanım verisi bulunamadı.");
                }

                // TODO: RDLC rapor oluşturma kodu buraya gelecek
                return new ErrorDataResult<byte[]>("RDLC rapor sistemi henüz implement edilmedi.");
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<byte[]>($"Rapor oluşturulurken hata: {ex.Message}");
            }
        }
    }
}
