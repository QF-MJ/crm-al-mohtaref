using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.AlMohtaref.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TaskType = table.Column<int>(type: "int", nullable: false),
                    TaskStatus = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "date", nullable: false),
                    EndTime = table.Column<DateTime>(type: "date", nullable: false),
                    ParentTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedToUserId  = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                    table.UniqueConstraint("AK_Tasks_Id_TaskType", x => new { x.Id, x.TaskType });
                    table.CheckConstraint("CK_Tasks_EndTime_GreaterThanOrEqual_StartTime", "[EndTime] >= [StartTime]");
                    table.CheckConstraint("CK_Tasks_ParentTask_NotSelf", "[ParentTaskId] IS NULL OR [ParentTaskId] <> [Id]");
                    table.ForeignKey(
                        name: "FK_Tasks_Tasks_ParentTaskId",
                        column: x => x.ParentTaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tasks_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tasks_Users_AssignedToUserId ",
                        column: x => x.AssignedToUserId ,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskType = table.Column<int>(type: "int", nullable: false, defaultValue: 8),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentTasks", x => x.Id);
                    table.CheckConstraint("CK_StudentTasks_TaskType_SubTaskOnly", "[TaskType] = 8");
                    table.ForeignKey(
                        name: "FK_StudentTasks_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentTasks_Tasks_TaskId_TaskType",
                        columns: x => new { x.TaskId, x.TaskType },
                        principalTable: "Tasks",
                        principalColumns: new[] { "Id", "TaskType" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentTasks_StudentId",
                table: "StudentTasks",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTasks_TaskId_StudentId",
                table: "StudentTasks",
                columns: new[] { "TaskId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentTasks_TaskId_TaskType",
                table: "StudentTasks",
                columns: new[] { "TaskId", "TaskType" });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_CreatedByUserId",
                table: "Tasks",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_AssignedToUserId ",
                table: "Tasks",
                column: "AssignedToUserId ");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Id_TaskType",
                table: "Tasks",
                columns: new[] { "Id", "TaskType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ParentTaskId",
                table: "Tasks",
                column: "ParentTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_StartTime_EndTime",
                table: "Tasks",
                columns: new[] { "StartTime", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_TaskStatus",
                table: "Tasks",
                column: "TaskStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_TaskType",
                table: "Tasks",
                column: "TaskType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentTasks");

            migrationBuilder.DropTable(
                name: "Tasks");
        }
    }
}
