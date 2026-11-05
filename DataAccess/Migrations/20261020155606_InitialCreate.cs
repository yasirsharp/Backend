using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AkademikPersonel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Unvan = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AkademikPersonel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AnnouncementReadStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnnouncementId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnnouncementReadStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssistantInteractions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PageContext = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FeatureUsed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsHelpful = table.Column<bool>(type: "bit", nullable: true),
                    ErrorReport = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FeedbackTimestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssistantInteractions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bolum",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bolum", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BolumAkademikPersoneller",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BolumId = table.Column<int>(type: "int", nullable: false),
                    AkademikPersonelId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BolumAkademikPersoneller", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Kod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DersBolum",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DersId = table.Column<int>(type: "int", nullable: false),
                    BolumId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DersBolum", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DersBolumAkademikPersonel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DersId = table.Column<int>(type: "int", nullable: false),
                    BolumId = table.Column<int>(type: "int", nullable: false),
                    AkademikPersonelId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DersBolumAkademikPersonel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Derslik",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Kapasite = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Derslik", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DerslikBolum",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DerslikId = table.Column<int>(type: "int", nullable: false),
                    BolumId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DerslikBolum", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ActionUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RelatedEntityId = table.Column<int>(type: "int", nullable: true),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ogrenci",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    OgrenciNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Soyad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BolumId = table.Column<int>(type: "int", nullable: false),
                    Sinif = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ogrenci", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OperationClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationClaims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SinavDerslik",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DerslikId = table.Column<int>(type: "int", nullable: false),
                    SinavDetayId = table.Column<int>(type: "int", nullable: false),
                    GozetmenId = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SinavDerslik", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SinavDetay",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DersBolumAkademikPersonelId = table.Column<int>(type: "int", nullable: false),
                    SinavTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SinavBaslangicSaati = table.Column<TimeOnly>(type: "time", nullable: false),
                    SinavBitisSaati = table.Column<TimeOnly>(type: "time", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SinavDetay", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAssistantPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    HasCompletedOnboarding = table.Column<bool>(type: "bit", nullable: false),
                    LastInteractionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PreferredLanguage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAssistantPreferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserOperationClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    OperationClaimId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOperationClaims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<byte[]>(type: "varbinary(128)", maxLength: 128, nullable: false),
                    PasswordSalt = table.Column<byte[]>(type: "varbinary(128)", maxLength: 128, nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    MustChangePassword = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AkademikPersonelMusaitlik",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AkademikPersonelId = table.Column<int>(type: "int", nullable: false),
                    BaslangicTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BaslangicSaati = table.Column<TimeSpan>(type: "time", nullable: true),
                    BitisSaati = table.Column<TimeSpan>(type: "time", nullable: true),
                    TekrarTipi = table.Column<int>(type: "int", nullable: false),
                    TekrarGunu = table.Column<int>(type: "int", nullable: true),
                    IsMusait = table.Column<bool>(type: "bit", nullable: false),
                    Neden = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AkademikPersonelMusaitlik", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AkademikPersonelMusaitlik_AkademikPersonel_AkademikPersonelId",
                        column: x => x.AkademikPersonelId,
                        principalTable: "AkademikPersonel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Announcements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    TargetAudience = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TargetBolumId = table.Column<int>(type: "int", nullable: true),
                    PublishDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ShowAsPopup = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announcements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Announcements_Bolum_TargetBolumId",
                        column: x => x.TargetBolumId,
                        principalTable: "Bolum",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "OperationClaims",
                columns: new[] { "Id", "CreatedDate", "Name", "Status", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "super.admin", true, null },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "akademik.personel", true, null },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ogrenci", true, null },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "bolum.gorevlisi", true, null },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "gorevli.personel", true, null },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Admin", true, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AkademikPersonel_UserId",
                table: "AkademikPersonel",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AkademikPersonelMusaitlik_AkademikPersonelId_BaslangicTarihi",
                table: "AkademikPersonelMusaitlik",
                columns: new[] { "AkademikPersonelId", "BaslangicTarihi" });

            migrationBuilder.CreateIndex(
                name: "IX_AnnouncementReadStatus_AnnouncementId_UserId",
                table: "AnnouncementReadStatus",
                columns: new[] { "AnnouncementId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_TargetBolumId",
                table: "Announcements",
                column: "TargetBolumId");

            migrationBuilder.CreateIndex(
                name: "IX_Bolum_Ad",
                table: "Bolum",
                column: "Ad",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BolumAkademikPersoneller_BolumId_AkademikPersonelId",
                table: "BolumAkademikPersoneller",
                columns: new[] { "BolumId", "AkademikPersonelId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ders_Kod",
                table: "Ders",
                column: "Kod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DersBolum_DersId_BolumId",
                table: "DersBolum",
                columns: new[] { "DersId", "BolumId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DersBolumAkademikPersonel_AkademikPersonelId",
                table: "DersBolumAkademikPersonel",
                column: "AkademikPersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_DersBolumAkademikPersonel_DersId_BolumId_AkademikPersonelId",
                table: "DersBolumAkademikPersonel",
                columns: new[] { "DersId", "BolumId", "AkademikPersonelId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Derslik_Ad",
                table: "Derslik",
                column: "Ad",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DerslikBolum_DerslikId_BolumId",
                table: "DerslikBolum",
                columns: new[] { "DerslikId", "BolumId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_Ogrenci_BolumId",
                table: "Ogrenci",
                column: "BolumId");

            migrationBuilder.CreateIndex(
                name: "IX_Ogrenci_OgrenciNo",
                table: "Ogrenci",
                column: "OgrenciNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ogrenci_UserId",
                table: "Ogrenci",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OperationClaims_Name",
                table: "OperationClaims",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SinavDerslik_DerslikId",
                table: "SinavDerslik",
                column: "DerslikId");

            migrationBuilder.CreateIndex(
                name: "IX_SinavDerslik_GozetmenId",
                table: "SinavDerslik",
                column: "GozetmenId");

            migrationBuilder.CreateIndex(
                name: "IX_SinavDerslik_SinavDetayId",
                table: "SinavDerslik",
                column: "SinavDetayId");

            migrationBuilder.CreateIndex(
                name: "IX_SinavDetay_DersBolumAkademikPersonelId",
                table: "SinavDetay",
                column: "DersBolumAkademikPersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_SinavDetay_SinavTarihi",
                table: "SinavDetay",
                column: "SinavTarihi");

            migrationBuilder.CreateIndex(
                name: "IX_UserOperationClaims_UserId_OperationClaimId",
                table: "UserOperationClaims",
                columns: new[] { "UserId", "OperationClaimId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AkademikPersonelMusaitlik");

            migrationBuilder.DropTable(
                name: "AnnouncementReadStatus");

            migrationBuilder.DropTable(
                name: "Announcements");

            migrationBuilder.DropTable(
                name: "AssistantInteractions");

            migrationBuilder.DropTable(
                name: "BolumAkademikPersoneller");

            migrationBuilder.DropTable(
                name: "Ders");

            migrationBuilder.DropTable(
                name: "DersBolum");

            migrationBuilder.DropTable(
                name: "DersBolumAkademikPersonel");

            migrationBuilder.DropTable(
                name: "Derslik");

            migrationBuilder.DropTable(
                name: "DerslikBolum");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Ogrenci");

            migrationBuilder.DropTable(
                name: "OperationClaims");

            migrationBuilder.DropTable(
                name: "SinavDerslik");

            migrationBuilder.DropTable(
                name: "SinavDetay");

            migrationBuilder.DropTable(
                name: "UserAssistantPreferences");

            migrationBuilder.DropTable(
                name: "UserOperationClaims");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "AkademikPersonel");

            migrationBuilder.DropTable(
                name: "Bolum");
        }
    }
}
