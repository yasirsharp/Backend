using Core.Entities;
using System;

namespace Entity.Concrete
{
    /// <summary>
    /// Kullanıcıların uygulama içinden gönderdiği hata bildirimi (Alt+Shift+H, telefonu sallama veya "Hata bildir").
    /// Ekran görüntüsü aynı satırda tutulur; listelerde yüklenmez.
    /// </summary>
    public class BugReport : IEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = "";
        public string Roles { get; set; } = "";
        public string Description { get; set; } = "";
        public string PageUrl { get; set; } = "";
        public string AppVersion { get; set; } = "";
        public string UserAgent { get; set; } = "";
        public string Viewport { get; set; } = "";
        /// <summary>İstemcideki son hata mesajları ve başarısız API istekleri (JSON metni)</summary>
        public string? ClientLog { get; set; }
        public byte[]? Screenshot { get; set; }
        public string? ScreenshotContentType { get; set; }
        /// <summary>yeni, inceleniyor, cozuldu</summary>
        public string State { get; set; } = BugReportStates.Yeni;
        public string? AdminNote { get; set; }
        public DateTime? ResolvedDate { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool Status { get; set; } = true;
    }

    public static class BugReportStates
    {
        public const string Yeni = "yeni";
        public const string Inceleniyor = "inceleniyor";
        public const string Cozuldu = "cozuldu";
        public static readonly string[] All = { Yeni, Inceleniyor, Cozuldu };
    }
}
