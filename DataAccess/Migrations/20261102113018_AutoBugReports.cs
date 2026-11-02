using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AutoBugReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Fingerprint",
                table: "BugReports",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSeenDate",
                table: "BugReports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OccurrenceCount",
                table: "BugReports",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "BugReports",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "kullanici");

            migrationBuilder.CreateIndex(
                name: "IX_BugReports_Fingerprint",
                table: "BugReports",
                column: "Fingerprint");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BugReports_Fingerprint",
                table: "BugReports");

            migrationBuilder.DropColumn(
                name: "Fingerprint",
                table: "BugReports");

            migrationBuilder.DropColumn(
                name: "LastSeenDate",
                table: "BugReports");

            migrationBuilder.DropColumn(
                name: "OccurrenceCount",
                table: "BugReports");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "BugReports");
        }
    }
}
