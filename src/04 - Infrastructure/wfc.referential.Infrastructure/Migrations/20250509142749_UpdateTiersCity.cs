using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wfc.referential.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTiersCity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Tax_EffectiveDate_ExpiryDate",
                table: "Taxes");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Tiers");

            migrationBuilder.DropColumn(
                name: "EffectiveDate",
                table: "Taxes");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "Taxes");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Taxes");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AgencyTiers");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "Taxes",
                newName: "Rate");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Tiers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Tiers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "Tiers",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<double>(
                name: "FixedAmount",
                table: "Taxes",
                type: "double precision",
                maxLength: 50,
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "Cities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "AgencyTiers",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "AgencyTiers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "Tiers");

            migrationBuilder.DropColumn(
                name: "FixedAmount",
                table: "Taxes");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "AgencyTiers");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "AgencyTiers");

            migrationBuilder.RenameColumn(
                name: "Rate",
                table: "Taxes",
                newName: "Value");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Tiers",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Tiers",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Tiers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveDate",
                table: "Taxes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "Taxes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Taxes",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Cities",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "AgencyTiers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Tax_EffectiveDate_ExpiryDate",
                table: "Taxes",
                sql: "\"EffectiveDate\" < \"ExpiryDate\"");
        }
    }
}
