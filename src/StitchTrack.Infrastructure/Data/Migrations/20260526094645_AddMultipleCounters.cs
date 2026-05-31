using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StitchTrack.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMultipleCounters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectCounters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CurrentCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectCounters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectCounters_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(@"
                INSERT INTO ProjectCounters (Id, ProjectId, Name, CurrentCount, SortOrder, CreatedAt)
                SELECT lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-4' ||
                       substr(lower(hex(randomblob(2))),2) || '-' ||
                       substr('89ab',abs(random()) % 4 + 1, 1) ||
                       substr(lower(hex(randomblob(2))),2) || '-' ||
                       lower(hex(randomblob(6))),
                       Id, 'Rows', CurrentCount, 0, datetime('now')
                FROM Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_CounterHistory_Projects_ProjectId",
                table: "CounterHistory");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "CounterHistory",
                newName: "ProjectCounterId");

            // Populate ProjectCounterId — column is renamed but still holds old ProjectId values,
            // so we match against ProjectCounters.ProjectId to find the right counter
            migrationBuilder.Sql(@"
                    UPDATE CounterHistory
                    SET ProjectCounterId = (
                        SELECT pc.Id FROM ProjectCounters pc
                        WHERE pc.ProjectId = CounterHistory.ProjectCounterId
                        AND pc.SortOrder = 0
                        LIMIT 1
                    )");

            migrationBuilder.RenameIndex(
                name: "IX_CounterHistory_ProjectId_ChangedAt",
                table: "CounterHistory",
                newName: "IX_CounterHistory_ProjectCounterId_ChangedAt");

            migrationBuilder.AddColumn<string>(
                name: "PrimaryCounterName",
                table: "Sessions",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectCounters_ProjectId_SortOrder",
                table: "ProjectCounters",
                columns: new[] { "ProjectId", "SortOrder" });

            migrationBuilder.AddForeignKey(
                name: "FK_CounterHistory_ProjectCounters_ProjectCounterId",
                table: "CounterHistory",
                column: "ProjectCounterId",
                principalTable: "ProjectCounters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CounterHistory_ProjectCounters_ProjectCounterId",
                table: "CounterHistory");

            migrationBuilder.DropTable(
                name: "ProjectCounters");

            migrationBuilder.DropColumn(
                name: "PrimaryCounterName",
                table: "Sessions");

            migrationBuilder.RenameColumn(
                name: "ProjectCounterId",
                table: "CounterHistory",
                newName: "ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_CounterHistory_ProjectCounterId_ChangedAt",
                table: "CounterHistory",
                newName: "IX_CounterHistory_ProjectId_ChangedAt");

            migrationBuilder.AddForeignKey(
                name: "FK_CounterHistory_Projects_ProjectId",
                table: "CounterHistory",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
