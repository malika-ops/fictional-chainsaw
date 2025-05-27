using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wfc.referential.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixSupportAccountDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "SupportAccounts");

            migrationBuilder.RenameColumn(
                name: "SupportAccountType",
                table: "SupportAccounts",
                newName: "Description");

            migrationBuilder.AlterColumn<Guid>(
                name: "PartnerId",
                table: "SupportAccounts",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "SupportAccounts",
                newName: "SupportAccountType");

            migrationBuilder.AlterColumn<Guid>(
                name: "PartnerId",
                table: "SupportAccounts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "SupportAccounts",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
