using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wfc.referential.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixRenamefielsAddPartnerAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AgencyTiers_AgencyId_TierId",
                table: "AgencyTiers");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "SupportAccounts");

            migrationBuilder.RenameColumn(
                name: "SupportAccountType",
                table: "SupportAccounts",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Partners",
                newName: "WithholdingTaxRate");

            migrationBuilder.RenameColumn(
                name: "SupportAccountType",
                table: "Partners",
                newName: "TransferType");

            migrationBuilder.RenameColumn(
                name: "RASRate",
                table: "Partners",
                newName: "ProfessionalTaxNumber");

            migrationBuilder.RenameColumn(
                name: "PaymentMode",
                table: "Partners",
                newName: "PhoneNumberContact");

            migrationBuilder.RenameColumn(
                name: "NetworkMode",
                table: "Partners",
                newName: "PersonType");

            migrationBuilder.RenameColumn(
                name: "Label",
                table: "Partners",
                newName: "Name");

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

            migrationBuilder.AddColumn<string>(
                name: "AuthenticationMode",
                table: "Partners",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Partners",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FunctionContact",
                table: "Partners",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HeadquartersAddress",
                table: "Partners",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HeadquartersCity",
                table: "Partners",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Partners",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MailContact",
                table: "Partners",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "NetworkModeId",
                table: "Partners",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PartnerTypeId",
                table: "Partners",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentModeId",
                table: "Partners",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SupportAccountTypeId",
                table: "Partners",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupportAccounts_SupportAccountTypeId",
                table: "SupportAccounts",
                column: "SupportAccountTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Partners_NetworkModeId",
                table: "Partners",
                column: "NetworkModeId");

            migrationBuilder.CreateIndex(
                name: "IX_Partners_PartnerTypeId",
                table: "Partners",
                column: "PartnerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Partners_PaymentModeId",
                table: "Partners",
                column: "PaymentModeId");

            migrationBuilder.CreateIndex(
                name: "IX_Partners_SupportAccountTypeId",
                table: "Partners",
                column: "SupportAccountTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Partners_ParamTypes_NetworkModeId",
                table: "Partners",
                column: "NetworkModeId",
                principalTable: "ParamTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Partners_ParamTypes_PartnerTypeId",
                table: "Partners",
                column: "PartnerTypeId",
                principalTable: "ParamTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Partners_ParamTypes_PaymentModeId",
                table: "Partners",
                column: "PaymentModeId",
                principalTable: "ParamTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Partners_ParamTypes_SupportAccountTypeId",
                table: "Partners",
                column: "SupportAccountTypeId",
                principalTable: "ParamTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
                name: "FK_Partners_ParamTypes_NetworkModeId",
                table: "Partners");

            migrationBuilder.DropForeignKey(
                name: "FK_Partners_ParamTypes_PartnerTypeId",
                table: "Partners");

            migrationBuilder.DropForeignKey(
                name: "FK_Partners_ParamTypes_PaymentModeId",
                table: "Partners");

            migrationBuilder.DropForeignKey(
                name: "FK_Partners_ParamTypes_SupportAccountTypeId",
                table: "Partners");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportAccounts_ParamTypes_SupportAccountTypeId",
                table: "SupportAccounts");

            migrationBuilder.DropIndex(
                name: "IX_SupportAccounts_SupportAccountTypeId",
                table: "SupportAccounts");

            migrationBuilder.DropIndex(
                name: "IX_Partners_NetworkModeId",
                table: "Partners");

            migrationBuilder.DropIndex(
                name: "IX_Partners_PartnerTypeId",
                table: "Partners");

            migrationBuilder.DropIndex(
                name: "IX_Partners_PaymentModeId",
                table: "Partners");

            migrationBuilder.DropIndex(
                name: "IX_Partners_SupportAccountTypeId",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "SupportAccountTypeId",
                table: "SupportAccounts");

            migrationBuilder.DropColumn(
                name: "AuthenticationMode",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "FunctionContact",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "HeadquartersAddress",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "HeadquartersCity",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "MailContact",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "NetworkModeId",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "PartnerTypeId",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "PaymentModeId",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "SupportAccountTypeId",
                table: "Partners");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "SupportAccounts",
                newName: "SupportAccountType");

            migrationBuilder.RenameColumn(
                name: "WithholdingTaxRate",
                table: "Partners",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "TransferType",
                table: "Partners",
                newName: "SupportAccountType");

            migrationBuilder.RenameColumn(
                name: "ProfessionalTaxNumber",
                table: "Partners",
                newName: "RASRate");

            migrationBuilder.RenameColumn(
                name: "PhoneNumberContact",
                table: "Partners",
                newName: "PaymentMode");

            migrationBuilder.RenameColumn(
                name: "PersonType",
                table: "Partners",
                newName: "NetworkMode");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Partners",
                newName: "Label");

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

            migrationBuilder.CreateIndex(
                name: "IX_AgencyTiers_AgencyId_TierId",
                table: "AgencyTiers",
                columns: new[] { "AgencyId", "TierId" },
                unique: true);
        }
    }
}
