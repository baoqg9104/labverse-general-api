using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Labverse.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionToActivityHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ActivityHistories",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "ActivityHistories");
        }
    }
}
