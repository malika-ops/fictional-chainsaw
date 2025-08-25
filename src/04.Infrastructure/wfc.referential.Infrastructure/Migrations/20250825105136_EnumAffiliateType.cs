using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wfc.referential.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnumAffiliateType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Affiliates_ParamTypes_AffiliateTypeId",
                table: "Affiliates");

            migrationBuilder.DropIndex(
                name: "IX_Affiliates_AffiliateTypeId",
                table: "Affiliates");

            migrationBuilder.DropColumn(
                name: "AffiliateTypeId",
                table: "Affiliates");

            migrationBuilder.AlterColumn<string>(
                name: "StampDutyMention",
                table: "Affiliates",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Logo",
                table: "Affiliates",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CancellationDay",
                table: "Affiliates",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccountingDocumentNumber",
                table: "Affiliates",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccountingAccountNumber",
                table: "Affiliates",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Abbreviation",
                table: "Affiliates",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AffiliateType",
                table: "Affiliates",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AffiliateType",
                table: "Affiliates");

            migrationBuilder.AlterColumn<string>(
                name: "StampDutyMention",
                table: "Affiliates",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Logo",
                table: "Affiliates",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "CancellationDay",
                table: "Affiliates",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "AccountingDocumentNumber",
                table: "Affiliates",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "AccountingAccountNumber",
                table: "Affiliates",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Abbreviation",
                table: "Affiliates",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AddColumn<Guid>(
                name: "AffiliateTypeId",
                table: "Affiliates",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Affiliates_AffiliateTypeId",
                table: "Affiliates",
                column: "AffiliateTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Affiliates_ParamTypes_AffiliateTypeId",
                table: "Affiliates",
                column: "AffiliateTypeId",
                principalTable: "ParamTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

        }
    }
}
