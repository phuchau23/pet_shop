using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodBooking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShipperLocationTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "shipper_current_lat",
                table: "orders",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "shipper_current_lng",
                table: "orders",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "shipper_location_updated_at",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "shipper_current_lat",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "shipper_current_lng",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "shipper_location_updated_at",
                table: "orders");
        }
    }
}
