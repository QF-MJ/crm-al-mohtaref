using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.AlMohtaref.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameExecutedByToAssignedToInTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Users_AssignedToUserId ",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "AssignedToUserId ",
                table: "Tasks",
                newName: "AssignedToUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_AssignedToUserId ",
                table: "Tasks",
                newName: "IX_Tasks_AssignedToUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Users_AssignedToUserId",
                table: "Tasks",
                column: "AssignedToUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Users_AssignedToUserId",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "AssignedToUserId",
                table: "Tasks",
                newName: "AssignedToUserId ");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_AssignedToUserId",
                table: "Tasks",
                newName: "IX_Tasks_AssignedToUserId ");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Users_AssignedToUserId ",
                table: "Tasks",
                column: "AssignedToUserId ",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
