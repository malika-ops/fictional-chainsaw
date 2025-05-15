using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wfc.referential.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixPartnerDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Partners_Cities_CityId",
                table: "Partners");

            migrationBuilder.DropForeignKey(
                name: "FK_Partners_Sectors_SectorId",
                table: "Partners");

            migrationBuilder.DropIndex(
                name: "IX_Partners_CityId",
                table: "Partners");

            migrationBuilder.DropIndex(
                name: "IX_Partners_IdentificationNumber",
                table: "Partners");

            migrationBuilder.DropIndex(
                name: "IX_Partners_SectorId",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "SectorId",
                table: "Partners");

            migrationBuilder.RenameColumn(
                name: "IdentificationNumber",
                table: "Partners",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "IdPartner",
                table: "Partners",
                newName: "TaxIdentificationNumber");

            migrationBuilder.AddColumn<Guid>(
                name: "ActivityAccountId",
                table: "Partners",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CommissionAccountId",
                table: "Partners",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IdParent",
                table: "Partners",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RASRate",
                table: "Partners",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "SupportAccountId",
                table: "Partners",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Partners_TaxIdentificationNumber",
                table: "Partners",
                column: "TaxIdentificationNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Partners_TaxIdentificationNumber",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "ActivityAccountId",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "CommissionAccountId",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "IdParent",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "RASRate",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "SupportAccountId",
                table: "Partners");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Partners",
                newName: "IdentificationNumber");

            migrationBuilder.RenameColumn(
                name: "TaxIdentificationNumber",
                table: "Partners",
                newName: "IdPartner");

            migrationBuilder.AddColumn<Guid>(
                name: "CityId",
                table: "Partners",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SectorId",
                table: "Partners",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Partners_CityId",
                table: "Partners",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Partners_IdentificationNumber",
                table: "Partners",
                column: "IdentificationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Partners_SectorId",
                table: "Partners",
                column: "SectorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Partners_Cities_CityId",
                table: "Partners",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Partners_Sectors_SectorId",
                table: "Partners",
                column: "SectorId",
                principalTable: "Sectors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
