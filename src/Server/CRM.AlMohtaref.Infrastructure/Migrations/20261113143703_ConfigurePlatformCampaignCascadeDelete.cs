using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.AlMohtaref.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfigurePlatformCampaignCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlatformCampaigns_Campaigns_CampaignId",
                table: "PlatformCampaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_PlatformCampaigns_Platforms_PlatformId",
                table: "PlatformCampaigns");

            migrationBuilder.AddForeignKey(
                name: "FK_PlatformCampaigns_Campaigns_CampaignId",
                table: "PlatformCampaigns",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlatformCampaigns_Platforms_PlatformId",
                table: "PlatformCampaigns",
                column: "PlatformId",
                principalTable: "Platforms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlatformCampaigns_Campaigns_CampaignId",
                table: "PlatformCampaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_PlatformCampaigns_Platforms_PlatformId",
                table: "PlatformCampaigns");

            migrationBuilder.AddForeignKey(
                name: "FK_PlatformCampaigns_Campaigns_CampaignId",
                table: "PlatformCampaigns",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlatformCampaigns_Platforms_PlatformId",
                table: "PlatformCampaigns",
                column: "PlatformId",
                principalTable: "Platforms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
