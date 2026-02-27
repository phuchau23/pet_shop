using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodBooking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAvailableSizesToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "available_sizes",
                table: "products",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "available_sizes",
                table: "products");
        }
    }
}
