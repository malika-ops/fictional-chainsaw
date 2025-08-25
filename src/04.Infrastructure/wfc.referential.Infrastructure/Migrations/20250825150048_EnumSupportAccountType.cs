using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wfc.referential.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnumSupportAccountType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportAccounts_ParamTypes_SupportAccountTypeId",
                table: "SupportAccounts");

            migrationBuilder.DropIndex(
                name: "IX_SupportAccounts_SupportAccountTypeId",
                table: "SupportAccounts");

            migrationBuilder.DropColumn(
                name: "SupportAccountTypeId",
                table: "SupportAccounts");

            migrationBuilder.AddColumn<string>(
                name: "SupportAccountType",
                table: "SupportAccounts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SupportAccountType",
                table: "SupportAccounts");

            migrationBuilder.AddColumn<Guid>(
                name: "SupportAccountTypeId",
                table: "SupportAccounts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupportAccounts_SupportAccountTypeId",
                table: "SupportAccounts",
                column: "SupportAccountTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportAccounts_ParamTypes_SupportAccountTypeId",
                table: "SupportAccounts",
                column: "SupportAccountTypeId",
                principalTable: "ParamTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
