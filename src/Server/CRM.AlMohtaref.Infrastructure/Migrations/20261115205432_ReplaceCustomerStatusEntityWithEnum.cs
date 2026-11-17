using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.AlMohtaref.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceCustomerStatusEntityWithEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CampaignLeads_LeadStatuses_StatusId",
                table: "CampaignLeads");

            migrationBuilder.DropTable(
                name: "LeadStatuses");

            migrationBuilder.DropIndex(
                name: "IX_CampaignLeads_StatusId",
                table: "CampaignLeads");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "CampaignLeads");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "CampaignLeads",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "CampaignLeads");

            migrationBuilder.AddColumn<Guid>(
                name: "StatusId",
                table: "CampaignLeads",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "LeadStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadStatuses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignLeads_StatusId",
                table: "CampaignLeads",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadStatuses_Name",
                table: "LeadStatuses",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignLeads_LeadStatuses_StatusId",
                table: "CampaignLeads",
                column: "StatusId",
                principalTable: "LeadStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
