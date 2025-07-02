using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wfc.referential.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceCotrolesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceControles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ControleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChannelId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExecOrder = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceControles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceControles_Controles_ControleId",
                        column: x => x.ControleId,
                        principalTable: "Controles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServiceControles_ParamTypes_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "ParamTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServiceControles_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceControles_ChannelId",
                table: "ServiceControles",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceControles_ControleId",
                table: "ServiceControles",
                column: "ControleId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceControles_ServiceId_ControleId_ChannelId",
                table: "ServiceControles",
                columns: new[] { "ServiceId", "ControleId", "ChannelId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceControles");
        }
    }
}
