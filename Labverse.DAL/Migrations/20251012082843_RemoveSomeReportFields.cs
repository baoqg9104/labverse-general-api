using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Labverse.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSomeReportFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LabId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Route",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "Reports");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LabId",
                table: "Reports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Route",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
