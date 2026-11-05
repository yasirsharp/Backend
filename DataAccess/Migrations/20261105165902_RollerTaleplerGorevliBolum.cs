using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RollerTaleplerGorevliBolum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KullaniciBolum",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BolumId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KullaniciBolum", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SinavDegisiklikTalepleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SinavDetayId = table.Column<int>(type: "int", nullable: false),
                    BolumId = table.Column<int>(type: "int", nullable: false),
                    SinavOzeti = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TalepEdenUserId = table.Column<int>(type: "int", nullable: false),
                    TalepEdenAd = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Gorev = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Neden = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    OnerilenTarih = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OnerilenBaslangic = table.Column<TimeOnly>(type: "time", nullable: true),
                    OnerilenBitis = table.Column<TimeOnly>(type: "time", nullable: true),
                    Durum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    YanitNotu = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    YanitlayanUserId = table.Column<int>(type: "int", nullable: true),
                    YanitlayanAd = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    YanitTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SinavDegisiklikTalepleri", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KullaniciBolum_UserId_BolumId",
                table: "KullaniciBolum",
                columns: new[] { "UserId", "BolumId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SinavDegisiklikTalepleri_BolumId_Durum",
                table: "SinavDegisiklikTalepleri",
                columns: new[] { "BolumId", "Durum" });

            migrationBuilder.CreateIndex(
                name: "IX_SinavDegisiklikTalepleri_TalepEdenUserId",
                table: "SinavDegisiklikTalepleri",
                column: "TalepEdenUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KullaniciBolum");

            migrationBuilder.DropTable(
                name: "SinavDegisiklikTalepleri");
        }
    }
}
