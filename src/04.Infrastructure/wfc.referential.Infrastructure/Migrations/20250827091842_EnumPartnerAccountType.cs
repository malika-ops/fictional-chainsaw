using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wfc.referential.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnumPartnerAccountType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartnerAccounts_ParamTypes_AccountTypeId",
                table: "PartnerAccounts");

            migrationBuilder.DropIndex(
                name: "IX_PartnerAccounts_AccountTypeId",
                table: "PartnerAccounts");

            migrationBuilder.DropColumn(
                name: "AccountTypeId",
                table: "PartnerAccounts");

            migrationBuilder.AddColumn<string>(
                name: "PartnerAccountType",
                table: "PartnerAccounts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PartnerAccountType",
                table: "PartnerAccounts");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountTypeId",
                table: "PartnerAccounts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_PartnerAccounts_AccountTypeId",
                table: "PartnerAccounts",
                column: "AccountTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PartnerAccounts_ParamTypes_AccountTypeId",
                table: "PartnerAccounts",
                column: "AccountTypeId",
                principalTable: "ParamTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
