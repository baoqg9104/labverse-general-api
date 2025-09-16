using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Labverse.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ExcludeIsActiveFromRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "RefreshTokens");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "RefreshTokens",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
