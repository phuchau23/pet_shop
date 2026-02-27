using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodBooking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEstimatedDeliveryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "shop_lng",
                table: "orders",
                type: "double precision",
                nullable: false,
                defaultValue: 106.809997,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldDefaultValue: 106.7749);

            migrationBuilder.AlterColumn<double>(
                name: "shop_lat",
                table: "orders",
                type: "double precision",
                nullable: false,
                defaultValue: 10.841449000000001,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldDefaultValue: 10.8506);

            migrationBuilder.AddColumn<int>(
                name: "estimated_delivery_minutes",
                table: "orders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "estimated_distance_meters",
                table: "orders",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "estimated_delivery_minutes",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "estimated_distance_meters",
                table: "orders");

            migrationBuilder.AlterColumn<double>(
                name: "shop_lng",
                table: "orders",
                type: "double precision",
                nullable: false,
                defaultValue: 106.7749,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldDefaultValue: 106.809997);

            migrationBuilder.AlterColumn<double>(
                name: "shop_lat",
                table: "orders",
                type: "double precision",
                nullable: false,
                defaultValue: 10.8506,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldDefaultValue: 10.841449000000001);
        }
    }
}
