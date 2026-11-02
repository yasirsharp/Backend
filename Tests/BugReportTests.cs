using System.Linq.Expressions;
using API.Services;
using Business.Concrete;
using Business.Helpers;
using Core.Utilites.Results.Pagination;
using DataAccess.Abstract;
using Entity.Concrete;
using Entity.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using MailKit.Security;
using Xunit;

namespace Tests
{
    public class BugReportTests
    {
        private static readonly byte[] Jpeg = { 0xFF, 0xD8, 0xFF, 0xE0, 0, 0 };
        private static readonly byte[] Png = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0 };

        private class FakeDal : IBugReportDal
        {
            public readonly List<BugReport> Items = new();
            public void Add(BugReport entity) { entity.Id = Items.Count + 1; Items.Add(entity); }
            public List<BugReport> GetAll(Expression<Func<BugReport, bool>>? filter = null) =>
                filter == null ? Items.ToList() : Items.Where(filter.Compile()).ToList();
            public BugReport? Get(Expression<Func<BugReport, bool>>? filter = null) => GetAll(filter).FirstOrDefault();
            public void Update(BugReport entity) { }
            public void Delete(BugReport entity) => Items.Remove(entity);
            public PagedResult<BugReport> GetPaged(PaginationParams p, Expression<Func<BugReport, bool>>? filter = null) => throw new NotImplementedException();
            public List<BugReportSummaryDto> GetSummaries(string? state = null, int? id = null) =>
                Items.Where(x => (state == null || x.State == state) && (id == null || x.Id == id))
                    .Select(x => new BugReportSummaryDto { Id = x.Id, State = x.State, HasScreenshot = x.Screenshot != null }).ToList();
            public (byte[] Data, string ContentType)? GetScreenshot(int id) => null;
            public int CountByState(string state) => Items.Count(x => x.State == state);
        }

        [Fact]
        public void Detects_jpeg_and_png_only()
        {
            Assert.Equal("image/jpeg", BugReportRules.DetectImageType(Jpeg));
            Assert.Equal("image/png", BugReportRules.DetectImageType(Png));
            Assert.Null(BugReportRules.DetectImageType("<svg onload=alert(1)>"u8.ToArray()));
            Assert.NotNull(BugReportRules.ValidateScreenshot(new byte[BugReportRules.MaxScreenshotBytes + 1]));
        }

        [Fact]
        public void Add_requires_description_and_valid_image()
        {
            var manager = new BugReportManager(new FakeDal());
            Assert.False(manager.Add(new BugReport { Description = "   " }).Success);
            Assert.False(manager.Add(new BugReport { Description = "hata", Screenshot = "GIF89a"u8.ToArray() }).Success);
        }

        [Fact]
        public void Add_clips_fields_and_starts_as_new()
        {
            var dal = new FakeDal();
            var result = new BugReportManager(dal).Add(new BugReport
            {
                Description = "  " + new string('x', 5000),
                PageUrl = new string('u', 900),
                Screenshot = Png,
                State = BugReportStates.Cozuldu,
                AdminNote = "istemci yazmamalı",
            });

            Assert.True(result.Success);
            var saved = dal.Items.Single();
            Assert.Equal(BugReportRules.MaxDescriptionChars, saved.Description.Length);
            Assert.Equal(500, saved.PageUrl.Length);
            Assert.Equal(BugReportStates.Yeni, saved.State);
            Assert.Null(saved.AdminNote);
            Assert.Equal("image/png", saved.ScreenshotContentType);
        }

        [Fact]
        public void State_update_sets_and_clears_resolved_date()
        {
            var dal = new FakeDal();
            var manager = new BugReportManager(dal);
            manager.Add(new BugReport { Description = "hata" });

            Assert.False(manager.UpdateState(1, new BugReportStateDto { State = "bilinmeyen" }).Success);
            Assert.True(manager.UpdateState(1, new BugReportStateDto { State = BugReportStates.Cozuldu, AdminNote = "düzeltildi" }).Success);
            Assert.NotNull(dal.Items[0].ResolvedDate);
            Assert.True(manager.UpdateState(1, new BugReportStateDto { State = BugReportStates.Inceleniyor }).Success);
            Assert.Null(dal.Items[0].ResolvedDate);
            Assert.Equal(0, manager.CountNew().Data);
        }

        [Fact]
        public void Mailer_is_skipped_without_smtp_settings()
        {
            var empty = new ConfigurationBuilder().Build();
            var mailer = new BugReportMailer(empty, NullLogger<BugReportMailer>.Instance);
            Assert.False(mailer.IsConfigured);
            mailer.SendInBackground(new BugReport { Description = "x" }, "https://ornek/yonetim"); // atlanır, hata vermez

            var onlyHost = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Email:Smtp:Host"] = "smtp.ornek" }).Build();
            Assert.False(new BugReportMailer(onlyHost, NullLogger<BugReportMailer>.Instance).IsConfigured);
        }

        private static IConfiguration Smtp(params (string Key, string Value)[] values) =>
            new ConfigurationBuilder().AddInMemoryCollection(values.ToDictionary(v => v.Key, v => (string?)v.Value)).Build();

        [Fact]
        public void Security_is_resolved_from_port_and_settings()
        {
            Assert.Equal(SecureSocketOptions.SslOnConnect, BugReportMailer.ResolveSecurity(Smtp(("Port", "465"))));
            Assert.Equal(SecureSocketOptions.StartTlsWhenAvailable, BugReportMailer.ResolveSecurity(Smtp(("Port", "587"))));
            Assert.Equal(SecureSocketOptions.None, BugReportMailer.ResolveSecurity(Smtp(("Port", "25"), ("EnableSsl", "false"))));
            Assert.Equal(SecureSocketOptions.StartTls, BugReportMailer.ResolveSecurity(Smtp(("Port", "587"), ("Security", "starttls"))));
            Assert.Equal(SecureSocketOptions.SslOnConnect, BugReportMailer.ResolveSecurity(Smtp(("Port", "2465"), ("Security", "SslOnConnect"))));
        }

        [Fact]
        public void Send_errors_are_explained_in_turkish()
        {
            Assert.Contains("parola reddedildi", BugReportMailer.Describe(new AuthenticationException("535")));
            Assert.Contains("465", BugReportMailer.Describe(new NotSupportedException("The SMTP server does not support the STARTTLS extension.")));
            Assert.Contains("bağlanılamadı", BugReportMailer.Describe(new System.Net.Sockets.SocketException(10061)));
        }

        [Fact]
        public void Paths_are_normalized_without_ids_and_query()
        {
            Assert.Equal("/api/sinavdetay/{id}", BugReportRules.NormalizePath("/api/SinavDetay/42?search=Ali"));
            Assert.Equal("/tanimlar/dersler", BugReportRules.NormalizePath("https://site/tanimlar/dersler#x"));
            Assert.Equal("/api/x/{id}/y", BugReportRules.NormalizePath("/api/x/3f2504e0-4f89-11d3-9a0c-0305e82c3301/y"));
        }

        [Fact]
        public void Fingerprint_is_stable_and_ignores_numbers_in_messages()
        {
            var a = BugReportRules.Fingerprint("istemci", "error", BugReportRules.NormalizeMessage("Sınav 12 bulunamadı"), "/x");
            var b = BugReportRules.Fingerprint("istemci", "error", BugReportRules.NormalizeMessage("Sınav 345 bulunamadı"), "/x");
            Assert.Equal(a, b);
            Assert.Equal(64, a.Length);
            Assert.NotEqual(a, BugReportRules.Fingerprint("istemci", "error", "başka", "/x"));
        }

        [Fact]
        public void App_frame_points_to_application_code()
        {
            try
            {
                Business.Helpers.BugReportRules.Clip(null, -1); // ArgumentOutOfRange: uygulama kodunda fırlar
            }
            catch (Exception ex)
            {
                Assert.StartsWith("Business.Helpers.BugReportRules.Clip", BugReportRules.AppFrame(ex));
            }
        }

        [Fact]
        public void Automatic_reports_are_counted_and_reopened_after_resolution()
        {
            var dal = new FakeDal();
            var manager = new BugReportManager(dal);
            BugReport Auto() => new() { Description = "Sunucu hatası: NullReferenceException", Fingerprint = "abc", Source = BugReportSources.Sunucu };

            var first = manager.RecordAutomatic(Auto());
            var second = manager.RecordAutomatic(Auto());
            Assert.True(first.Data.IsNew);
            Assert.False(second.Data.IsNew);
            Assert.Equal(first.Data.Id, second.Data.Id);
            Assert.Equal(2, dal.Items.Single().OccurrenceCount);
            Assert.Equal(BugReportSources.Sunucu, dal.Items.Single().Source);

            manager.UpdateState(first.Data.Id, new BugReportStateDto { State = BugReportStates.Cozuldu });
            var again = manager.RecordAutomatic(Auto());
            Assert.True(again.Data.IsNew);
            Assert.Contains("tekrar etti", dal.Items.Last().AdminNote);

            Assert.False(manager.RecordAutomatic(new BugReport { Description = "x" }).Success); // parmak izi zorunlu
        }

        [Fact]
        public void Manual_reports_cannot_pose_as_automatic()
        {
            var dal = new FakeDal();
            new BugReportManager(dal).Add(new BugReport { Description = "elle", Source = BugReportSources.Sunucu, Fingerprint = "abc", OccurrenceCount = 99 });
            Assert.Equal(BugReportSources.Kullanici, dal.Items[0].Source);
            Assert.Null(dal.Items[0].Fingerprint);
            Assert.Equal(1, dal.Items[0].OccurrenceCount);
        }

        private class RecordingReporter : Core.Utilities.Reporting.IUnhandledExceptionReporter
        {
            public readonly List<(Exception, string?)> Calls = new();
            public void Report(Exception exception, string? context = null) => Calls.Add((exception, context));
        }

        [Fact]
        public void Caught_business_exceptions_are_reported_with_user_message()
        {
            var reporter = new RecordingReporter();
            var previous = Core.Utilities.Results.ExceptionLogger.Reporter;
            Core.Utilities.Results.ExceptionLogger.Reporter = reporter;
            try
            {
                var message = Core.Utilities.Results.ExceptionLogger.Log(new InvalidOperationException("db"), "Sınav eklenemedi.");
                Assert.Equal("Sınav eklenemedi.", message);
                Assert.Equal("Sınav eklenemedi.", reporter.Calls.Single().Item2);
            }
            finally
            {
                Core.Utilities.Results.ExceptionLogger.Reporter = previous;
            }
        }

        [Fact]
        public void Automation_key_requires_long_exact_match()
        {
            var key = new string('k', 40);
            Assert.True(API.Controllers.AutomationKeyFilter.KeyMatches(key, key));
            Assert.False(API.Controllers.AutomationKeyFilter.KeyMatches(key, key + "x"));
            Assert.False(API.Controllers.AutomationKeyFilter.KeyMatches(key, null));
            Assert.False(API.Controllers.AutomationKeyFilter.KeyMatches("kisa", "kisa")); // 32 karakterden kısa anahtar kabul edilmez
        }

        [Fact]
        public void Mail_body_contains_context_and_link()
        {
            var body = BugReportMailer.BuildBody(new BugReport
            {
                Id = 7, Description = "Takvim açılmıyor", UserName = "Ali Veli", Roles = "Admin",
                PageUrl = "https://site/sinav-takvimi", AppVersion = "v0.11.0", Screenshot = Jpeg,
            }, "https://site/yonetim/hata-bildirimleri?id=7");
            Assert.Contains("Takvim açılmıyor", body);
            Assert.Contains("https://site/sinav-takvimi", body);
            Assert.Contains("ekte", body);
            Assert.Contains("hata-bildirimleri?id=7", body);
        }
    }
}
