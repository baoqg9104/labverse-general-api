using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Labverse.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnsToLabTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MdPath",
                table: "Labs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MdPublicUrl",
                table: "Labs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Labs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MdPath",
                table: "Labs");

            migrationBuilder.DropColumn(
                name: "MdPublicUrl",
                table: "Labs");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Labs");
        }
    }
}
