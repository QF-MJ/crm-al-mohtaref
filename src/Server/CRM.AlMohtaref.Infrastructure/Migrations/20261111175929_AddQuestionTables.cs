using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.AlMohtaref.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PersonId",
                table: "People",
                newName: "Id");

            migrationBuilder.AddColumn<Guid>(
                name: "PlatformCampaignId",
                table: "CampaignLeads",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Fields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fields", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Options",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OptionText = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Options", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OptionSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentHash = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptionSets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Platforms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platforms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuestionConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Target = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionConfigurations_Fields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "Fields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OptionTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TagId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptionTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptionTags_Options_OptionId",
                        column: x => x.OptionId,
                        principalTable: "Options",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OptionTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OptionSetItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OptionSetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptionSetItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptionSetItems_OptionSets_OptionSetId",
                        column: x => x.OptionSetId,
                        principalTable: "OptionSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OptionSetItems_Options_OptionId",
                        column: x => x.OptionId,
                        principalTable: "Options",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlatformCampaigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlatformId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformCampaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformCampaigns_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlatformCampaigns_Platforms_PlatformId",
                        column: x => x.PlatformId,
                        principalTable: "Platforms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_QuestionConfigurations_ConfigurationId",
                        column: x => x.ConfigurationId,
                        principalTable: "QuestionConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuestionCampaigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OptionSetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionCampaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionCampaigns_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuestionCampaigns_OptionSets_OptionSetId",
                        column: x => x.OptionSetId,
                        principalTable: "OptionSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuestionCampaigns_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignLeads_PlatformCampaignId",
                table: "CampaignLeads",
                column: "PlatformCampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_Fields_Name",
                table: "Fields",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Options_OptionText",
                table: "Options",
                column: "OptionText",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OptionSetItems_OptionId",
                table: "OptionSetItems",
                column: "OptionId");

            migrationBuilder.CreateIndex(
                name: "IX_OptionSetItems_OptionSetId_OptionId",
                table: "OptionSetItems",
                columns: new[] { "OptionSetId", "OptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OptionSets_ContentHash",
                table: "OptionSets",
                column: "ContentHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OptionTags_OptionId_TagId",
                table: "OptionTags",
                columns: new[] { "OptionId", "TagId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OptionTags_TagId",
                table: "OptionTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformCampaigns_CampaignId",
                table: "PlatformCampaigns",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformCampaigns_PlatformId_CampaignId",
                table: "PlatformCampaigns",
                columns: new[] { "PlatformId", "CampaignId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Platforms_Name",
                table: "Platforms",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionCampaigns_CampaignId_QuestionId",
                table: "QuestionCampaigns",
                columns: new[] { "CampaignId", "QuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionCampaigns_OptionSetId",
                table: "QuestionCampaigns",
                column: "OptionSetId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionCampaigns_QuestionId",
                table: "QuestionCampaigns",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionConfigurations_FieldId",
                table: "QuestionConfigurations",
                column: "FieldId",
                unique: true,
                filter: "[FieldId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_ConfigurationId",
                table: "Questions",
                column: "ConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_Text",
                table: "Questions",
                column: "Text",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignLeads_PlatformCampaigns_PlatformCampaignId",
                table: "CampaignLeads",
                column: "PlatformCampaignId",
                principalTable: "PlatformCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CampaignLeads_PlatformCampaigns_PlatformCampaignId",
                table: "CampaignLeads");

            migrationBuilder.DropTable(
                name: "OptionSetItems");

            migrationBuilder.DropTable(
                name: "OptionTags");

            migrationBuilder.DropTable(
                name: "PlatformCampaigns");

            migrationBuilder.DropTable(
                name: "QuestionCampaigns");

            migrationBuilder.DropTable(
                name: "Options");

            migrationBuilder.DropTable(
                name: "Platforms");

            migrationBuilder.DropTable(
                name: "OptionSets");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "QuestionConfigurations");

            migrationBuilder.DropTable(
                name: "Fields");

            migrationBuilder.DropIndex(
                name: "IX_CampaignLeads_PlatformCampaignId",
                table: "CampaignLeads");

            migrationBuilder.DropColumn(
                name: "PlatformCampaignId",
                table: "CampaignLeads");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "People",
                newName: "PersonId");
        }
    }
}
