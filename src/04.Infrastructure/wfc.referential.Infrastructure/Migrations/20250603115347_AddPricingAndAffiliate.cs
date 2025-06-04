using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wfc.referential.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPricingAndAffiliate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Affiliates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Abbreviation = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    OpeningDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancellationDay = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Logo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ThresholdBilling = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    AccountingDocumentNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AccountingAccountNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StampDutyMention = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: false),
                    AffiliateTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Affiliates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Affiliates_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Affiliates_ParamTypes_AffiliateTypeId",
                        column: x => x.AffiliateTypeId,
                        principalTable: "ParamTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pricings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Channel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MinimumAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MaximumAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FixedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Rate = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CorridorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    AffiliateId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pricings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pricings_Affiliates_AffiliateId",
                        column: x => x.AffiliateId,
                        principalTable: "Affiliates",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Pricings_Corridors_CorridorId",
                        column: x => x.CorridorId,
                        principalTable: "Corridors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Pricings_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Affiliates_AffiliateTypeId",
                table: "Affiliates",
                column: "AffiliateTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Affiliates_Code",
                table: "Affiliates",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Affiliates_CountryId",
                table: "Affiliates",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Pricings_AffiliateId",
                table: "Pricings",
                column: "AffiliateId");

            migrationBuilder.CreateIndex(
                name: "IX_Pricings_Code",
                table: "Pricings",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pricings_CorridorId",
                table: "Pricings",
                column: "CorridorId");

            migrationBuilder.CreateIndex(
                name: "IX_Pricings_ServiceId",
                table: "Pricings",
                column: "ServiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pricings");

            migrationBuilder.DropTable(
                name: "Affiliates");
        }
    }
}
