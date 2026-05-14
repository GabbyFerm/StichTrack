using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StitchTrack.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReplacePatternFilesWithProjectFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create the new table first
            migrationBuilder.CreateTable(
                name: "ProjectFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileType = table.Column<int>(type: "INTEGER", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    FileUrl = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectFiles_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 2. Copy existing pattern data before dropping the old table
            migrationBuilder.Sql(
                @"INSERT INTO ProjectFiles (Id, ProjectId, FileType, FileName, FilePath, FileUrl,
                  FileSizeBytes, ContentType, UploadedAt)
                  SELECT Id, ProjectId, 0, FileName, FilePath, FileUrl,
                  FileSizeBytes, ContentType, UploadedAt FROM PatternFiles");

            // 3. Now safe to drop
            migrationBuilder.DropTable(
                name: "PatternFiles");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectFiles_ProjectId_FileType",
                table: "ProjectFiles",
                columns: new[] { "ProjectId", "FileType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectFiles");

            migrationBuilder.CreateTable(
                name: "PatternFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    FileUrl = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_PatternFiles_ProjectId",
                table: "PatternFiles",
                column: "ProjectId");
        }
    }
}
