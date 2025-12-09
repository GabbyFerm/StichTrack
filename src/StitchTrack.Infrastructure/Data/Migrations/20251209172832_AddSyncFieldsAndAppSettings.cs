using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StitchTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSyncFieldsAndAppSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CloudFileId",
                table: "Projects",
                type: "TEXT",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncedAt",
                table: "Projects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SyncVersion",
                table: "Projects",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsFirstRun = table.Column<bool>(type: "INTEGER", nullable: false),
                    FirstRunCompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SyncEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    SyncProvider = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    LastSuccessfulSync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Theme = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    HapticFeedbackEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    ProjectCreationCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AppSettings",
                columns: new[] { "Id", "FirstRunCompletedAt", "HapticFeedbackEnabled", "IsFirstRun", "LastSuccessfulSync", "ProjectCreationCount", "SyncEnabled", "SyncProvider", "Theme" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), null, true, true, null, 0, false, null, "Auto" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropColumn(
                name: "CloudFileId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "SyncVersion",
                table: "Projects");
        }
    }
}
