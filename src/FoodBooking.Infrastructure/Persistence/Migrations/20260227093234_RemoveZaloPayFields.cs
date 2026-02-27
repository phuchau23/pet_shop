using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodBooking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveZaloPayFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "zalopay_app_trans_id",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "zalopay_order_id",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "zalopay_payment_url",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "zalopay_qr_code_url",
                table: "payments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "zalopay_app_trans_id",
                table: "payments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "zalopay_order_id",
                table: "payments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "zalopay_payment_url",
                table: "payments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "zalopay_qr_code_url",
                table: "payments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
