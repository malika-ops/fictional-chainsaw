using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wfc.referential.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAgenciesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
               name: "SupportAccountId",
               table: "Agencies");

            migrationBuilder.AddColumn<Guid>(
                name: "SupportAccountId",
                table: "Agencies",
                type: "uuid",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "PartnerId",
                table: "Agencies");

            migrationBuilder.AddColumn<Guid>(
                name: "PartnerId",
                table: "Agencies",
                type: "uuid",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "MoneyGramPassword",
                table: "Agencies");

            migrationBuilder.DropColumn(
                name: "MoneyGramReferenceNumber",
                table: "Agencies");

            migrationBuilder.DropColumn(
                name: "PermissionOfficeChange",
                table: "Agencies");


            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Agencies",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Agencies",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Address2",
                table: "Agencies",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address1",
                table: "Agencies",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "CashTransporter",
                table: "Agencies",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpenseFundAccountNumber",
                table: "Agencies",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpenseFundAccountingSheet",
                table: "Agencies",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FundingThreshold",
                table: "Agencies",
                type: "numeric(18,3)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FundingTypeId",
                table: "Agencies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MadAccount",
                table: "Agencies",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TokenUsageStatusId",
                table: "Agencies",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Agencies_FundingTypeId",
                table: "Agencies",
                column: "FundingTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Agencies_PartnerId",
                table: "Agencies",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Agencies_SupportAccountId",
                table: "Agencies",
                column: "SupportAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Agencies_TokenUsageStatusId",
                table: "Agencies",
                column: "TokenUsageStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Agencies_ParamTypes_FundingTypeId",
                table: "Agencies",
                column: "FundingTypeId",
                principalTable: "ParamTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Agencies_ParamTypes_TokenUsageStatusId",
                table: "Agencies",
                column: "TokenUsageStatusId",
                principalTable: "ParamTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Agencies_Partners_PartnerId",
                table: "Agencies",
                column: "PartnerId",
                principalTable: "Partners",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Agencies_SupportAccounts_SupportAccountId",
                table: "Agencies",
                column: "SupportAccountId",
                principalTable: "SupportAccounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Agencies_ParamTypes_FundingTypeId",
                table: "Agencies");

            migrationBuilder.DropForeignKey(
                name: "FK_Agencies_ParamTypes_TokenUsageStatusId",
                table: "Agencies");

            migrationBuilder.DropForeignKey(
                name: "FK_Agencies_Partners_PartnerId",
                table: "Agencies");

            migrationBuilder.DropForeignKey(
                name: "FK_Agencies_SupportAccounts_SupportAccountId",
                table: "Agencies");

            migrationBuilder.DropIndex(
                name: "IX_Agencies_FundingTypeId",
                table: "Agencies");

            migrationBuilder.DropIndex(
                name: "IX_Agencies_PartnerId",
                table: "Agencies");

            migrationBuilder.DropIndex(
                name: "IX_Agencies_SupportAccountId",
                table: "Agencies");

            migrationBuilder.DropIndex(
                name: "IX_Agencies_TokenUsageStatusId",
                table: "Agencies");

            migrationBuilder.DropColumn(
                name: "CashTransporter",
                table: "Agencies");

            migrationBuilder.DropColumn(
                name: "ExpenseFundAccountNumber",
                table: "Agencies");

            migrationBuilder.DropColumn(
                name: "ExpenseFundAccountingSheet",
                table: "Agencies");

            migrationBuilder.DropColumn(
                name: "FundingThreshold",
                table: "Agencies");

            migrationBuilder.DropColumn(
                name: "FundingTypeId",
                table: "Agencies");

            migrationBuilder.DropColumn(
                name: "MadAccount",
                table: "Agencies");

            migrationBuilder.DropColumn(
                name: "TokenUsageStatusId",
                table: "Agencies");

            migrationBuilder.AlterColumn<string>(
                name: "SupportAccountId",
                table: "Agencies",
                type: "text",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PartnerId",
                table: "Agencies",
                type: "text",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Agencies",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Agencies",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Address2",
                table: "Agencies",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address1",
                table: "Agencies",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<string>(
                name: "MoneyGramPassword",
                table: "Agencies",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MoneyGramReferenceNumber",
                table: "Agencies",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PermissionOfficeChange",
                table: "Agencies",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");
        }
    }
}
