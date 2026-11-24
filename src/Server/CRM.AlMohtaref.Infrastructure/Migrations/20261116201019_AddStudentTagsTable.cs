using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.AlMohtaref.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentTagsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactTags_Students_StudentId",
                table: "ContactTags");

            migrationBuilder.DropIndex(
                name: "IX_ContactTags_StudentId",
                table: "ContactTags");

            migrationBuilder.DropIndex(
                name: "IX_ContactTags_TagId_StudentId_ContactId",
                table: "ContactTags");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ContactTags_Target",
                table: "ContactTags");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "ContactTags");

            migrationBuilder.CreateTable(
                name: "StudentTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TagId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentTags_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactTags_TagId_ContactId",
                table: "ContactTags",
                columns: new[] { "TagId", "ContactId" },
                unique: true,
                filter: "[ContactId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTags_StudentId",
                table: "StudentTags",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTags_TagId_StudentId",
                table: "StudentTags",
                columns: new[] { "TagId", "StudentId" },
                unique: true,
                filter: "[StudentId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentTags");

            migrationBuilder.DropIndex(
                name: "IX_ContactTags_TagId_ContactId",
                table: "ContactTags");

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                table: "ContactTags",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactTags_StudentId",
                table: "ContactTags",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactTags_TagId_StudentId_ContactId",
                table: "ContactTags",
                columns: new[] { "TagId", "StudentId", "ContactId" },
                unique: true,
                filter: "[StudentId] IS NOT NULL AND [ContactId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ContactTags_Target",
                table: "ContactTags",
                sql: "([StudentId] IS NOT NULL AND [ContactId] IS NULL) OR ([StudentId] IS NULL AND [ContactId] IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactTags_Students_StudentId",
                table: "ContactTags",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
