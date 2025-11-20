using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StitchTrack.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                PasswordHash = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                DisplayName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Projects",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                CurrentCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                ColorHex = table.Column<string>(type: "TEXT", maxLength: 9, nullable: true),
                TotalRows = table.Column<int>(type: "INTEGER", nullable: true),
                RowsPerRepeat = table.Column<int>(type: "INTEGER", nullable: true),
                Notes = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                IsArchived = table.Column<bool>(type: "INTEGER", nullable: false),
                ImagePath = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                ImageUrl = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Projects", x => x.Id);
                table.ForeignKey(
                    name: "FK_Projects_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "CounterHistory",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                OldValue = table.Column<int>(type: "INTEGER", nullable: false),
                NewValue = table.Column<int>(type: "INTEGER", nullable: false),
                ChangedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CounterHistory", x => x.Id);
                table.ForeignKey(
                    name: "FK_CounterHistory_Projects_ProjectId",
                    column: x => x.ProjectId,
                    principalTable: "Projects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PatternFiles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                FileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                FilePath = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                FileUrl = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                FileSizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                ContentType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PatternFiles", x => x.Id);
                table.ForeignKey(
                    name: "FK_PatternFiles_Projects_ProjectId",
                    column: x => x.ProjectId,
                    principalTable: "Projects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Reminders",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                IntervalMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                LastTriggeredAt = table.Column<DateTime>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Reminders", x => x.Id);
                table.ForeignKey(
                    name: "FK_Reminders_Projects_ProjectId",
                    column: x => x.ProjectId,
                    principalTable: "Projects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "RowNotes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                RowNumber = table.Column<int>(type: "INTEGER", nullable: false),
                NoteText = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RowNotes", x => x.Id);
                table.ForeignKey(
                    name: "FK_RowNotes_Projects_ProjectId",
                    column: x => x.ProjectId,
                    principalTable: "Projects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Sessions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                EndedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                DurationSeconds = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                StartingRowCount = table.Column<int>(type: "INTEGER", nullable: true),
                EndingRowCount = table.Column<int>(type: "INTEGER", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Sessions", x => x.Id);
                table.ForeignKey(
                    name: "FK_Sessions_Projects_ProjectId",
                    column: x => x.ProjectId,
                    principalTable: "Projects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_CounterHistory_ProjectId_ChangedAt",
            table: "CounterHistory",
            columns: new[] { "ProjectId", "ChangedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_PatternFiles_ProjectId",
            table: "PatternFiles",
            column: "ProjectId");

        migrationBuilder.CreateIndex(
            name: "IX_Projects_IsArchived",
            table: "Projects",
            column: "IsArchived");

        migrationBuilder.CreateIndex(
            name: "IX_Projects_UpdatedAt",
            table: "Projects",
            column: "UpdatedAt");

        migrationBuilder.CreateIndex(
            name: "IX_Projects_UserId",
            table: "Projects",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Reminders_ProjectId",
            table: "Reminders",
            column: "ProjectId");

        migrationBuilder.CreateIndex(
            name: "IX_RowNotes_ProjectId_RowNumber",
            table: "RowNotes",
            columns: new[] { "ProjectId", "RowNumber" });

        migrationBuilder.CreateIndex(
            name: "IX_Sessions_ProjectId",
            table: "Sessions",
            column: "ProjectId");

        migrationBuilder.CreateIndex(
            name: "IX_Sessions_StartedAt",
            table: "Sessions",
            column: "StartedAt");

        migrationBuilder.CreateIndex(
            name: "IX_Users_Email",
            table: "Users",
            column: "Email",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "CounterHistory");

        migrationBuilder.DropTable(
            name: "PatternFiles");

        migrationBuilder.DropTable(
            name: "Reminders");

        migrationBuilder.DropTable(
            name: "RowNotes");

        migrationBuilder.DropTable(
            name: "Sessions");

        migrationBuilder.DropTable(
            name: "Projects");

        migrationBuilder.DropTable(
            name: "Users");
    }
}
