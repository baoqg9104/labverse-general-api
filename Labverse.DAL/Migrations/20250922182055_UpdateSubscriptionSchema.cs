using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Labverse.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSubscriptionSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DurationInDays",
                table: "Subscriptions",
                newName: "DurationValue");

            migrationBuilder.AddColumn<string>(
                name: "DurationUnit",
                table: "Subscriptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationUnit",
                table: "Subscriptions");

            migrationBuilder.RenameColumn(
                name: "DurationValue",
                table: "Subscriptions",
                newName: "DurationInDays");
        }
    }
}
