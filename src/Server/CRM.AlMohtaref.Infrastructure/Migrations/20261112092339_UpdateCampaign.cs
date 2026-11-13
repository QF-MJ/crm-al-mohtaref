using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.AlMohtaref.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCampaign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Campaigns_Title",
                table: "Campaigns");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_Title",
                table: "Campaigns",
                column: "Title",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Campaigns_Title",
                table: "Campaigns");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_Title",
                table: "Campaigns",
                column: "Title");
        }
    }
}
