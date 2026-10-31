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
