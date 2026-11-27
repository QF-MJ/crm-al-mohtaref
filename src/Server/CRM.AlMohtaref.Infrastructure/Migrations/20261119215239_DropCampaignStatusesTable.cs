using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.AlMohtaref.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropCampaignStatusesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_CampaignStatuses_CampaginStatusId",
                table: "Campaigns");

            migrationBuilder.DropTable(
                name: "CampaignStatuses");

            migrationBuilder.DropIndex(
                name: "IX_Campaigns_CampaginStatusId",
                table: "Campaigns");

            migrationBuilder.AddColumn<int>(
                name: "CampaignStatus",
                table: "Campaigns",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CampaignStatus",
                table: "Campaigns");

            migrationBuilder.CreateTable(
                name: "CampaignStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignStatuses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_CampaginStatusId",
                table: "Campaigns",
                column: "CampaginStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_CampaignStatuses_CampaginStatusId",
                table: "Campaigns",
                column: "CampaginStatusId",
                principalTable: "CampaignStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
