using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wfc.referential.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFlowDirectionService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FlowDirection",
                table: "Services",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FlowDirection",
                table: "Services");
        }
    }
}
