// Copyright (c) 2026 Gabriella Frank Ferm / Frank Ferm Design. All rights reserved.
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StitchTrack.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectTagsAndNeedleSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NeedleOrHookSize",
                table: "Projects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProjectTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ColorIndex = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectTags_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTags_ProjectId",
                table: "ProjectTags",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectTags");

            migrationBuilder.DropColumn(
                name: "NeedleOrHookSize",
                table: "Projects");
        }
    }
}
