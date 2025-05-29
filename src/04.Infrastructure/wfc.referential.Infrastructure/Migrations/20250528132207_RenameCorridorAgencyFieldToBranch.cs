using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wfc.referential.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameCorridorAgencyFieldToBranch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Corridors_Agencies_DestinationAgencyId",
                table: "Corridors");

            migrationBuilder.DropForeignKey(
                name: "FK_Corridors_Agencies_SourceAgencyId",
                table: "Corridors");

            migrationBuilder.RenameColumn(
                name: "SourceAgencyId",
                table: "Corridors",
                newName: "SourceBranchId");

            migrationBuilder.RenameColumn(
                name: "DestinationAgencyId",
                table: "Corridors",
                newName: "DestinationBranchId");

            migrationBuilder.RenameIndex(
                name: "IX_Corridors_SourceAgencyId",
                table: "Corridors",
                newName: "IX_Corridors_SourceBranchId");

            migrationBuilder.RenameIndex(
                name: "IX_Corridors_DestinationAgencyId",
                table: "Corridors",
                newName: "IX_Corridors_DestinationBranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Corridors_Agencies_DestinationBranchId",
                table: "Corridors",
                column: "DestinationBranchId",
                principalTable: "Agencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Corridors_Agencies_SourceBranchId",
                table: "Corridors",
                column: "SourceBranchId",
                principalTable: "Agencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Corridors_Agencies_DestinationBranchId",
                table: "Corridors");

            migrationBuilder.DropForeignKey(
                name: "FK_Corridors_Agencies_SourceBranchId",
                table: "Corridors");

            migrationBuilder.RenameColumn(
                name: "SourceBranchId",
                table: "Corridors",
                newName: "SourceAgencyId");

            migrationBuilder.RenameColumn(
                name: "DestinationBranchId",
                table: "Corridors",
                newName: "DestinationAgencyId");

            migrationBuilder.RenameIndex(
                name: "IX_Corridors_SourceBranchId",
                table: "Corridors",
                newName: "IX_Corridors_SourceAgencyId");

            migrationBuilder.RenameIndex(
                name: "IX_Corridors_DestinationBranchId",
                table: "Corridors",
                newName: "IX_Corridors_DestinationAgencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Corridors_Agencies_DestinationAgencyId",
                table: "Corridors",
                column: "DestinationAgencyId",
                principalTable: "Agencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Corridors_Agencies_SourceAgencyId",
                table: "Corridors",
                column: "SourceAgencyId",
                principalTable: "Agencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
