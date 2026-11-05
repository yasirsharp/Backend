using System;

namespace Entity.DTOs
{
    /// <summary>Hata bildirimi listesi/ayrıntısı (ekran görüntüsünün kendisi ayrı uçtan indirilir)</summary>
    public class BugReportSummaryDto
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
        public string? ClientLog { get; set; }
        public bool HasScreenshot { get; set; }
        public string State { get; set; } = "";
        public string? AdminNote { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string Source { get; set; } = "";
        public int OccurrenceCount { get; set; }
        public DateTime? LastSeenDate { get; set; }
    }

    /// <summary>Sayfada yakalanan beklenmeyen hata (istemci kendiliğinden gönderir)</summary>
    public class ClientErrorDto
    {
        public string? Kind { get; set; }
        public string? Message { get; set; }
        public string? Stack { get; set; }
        public string? PageUrl { get; set; }
        public string? AppVersion { get; set; }
        public string? Viewport { get; set; }
    }

    public class BugReportStateDto
    {
        public string State { get; set; } = "";
        public string? AdminNote { get; set; }
    }
}
