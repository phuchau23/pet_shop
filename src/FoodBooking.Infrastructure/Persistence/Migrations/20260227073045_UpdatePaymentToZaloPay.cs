using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodBooking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePaymentToZaloPay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "momo_request_id",
                table: "payments",
                newName: "zalopay_order_id");

            migrationBuilder.RenameColumn(
                name: "momo_qr_code_url",
                table: "payments",
                newName: "zalopay_qr_code_url");

            migrationBuilder.RenameColumn(
                name: "momo_order_id",
                table: "payments",
                newName: "zalopay_app_trans_id");

            migrationBuilder.RenameColumn(
                name: "momo_deep_link",
                table: "payments",
                newName: "zalopay_payment_url");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "zalopay_qr_code_url",
                table: "payments",
                newName: "momo_qr_code_url");

            migrationBuilder.RenameColumn(
                name: "zalopay_payment_url",
                table: "payments",
                newName: "momo_deep_link");

            migrationBuilder.RenameColumn(
                name: "zalopay_order_id",
                table: "payments",
                newName: "momo_request_id");

            migrationBuilder.RenameColumn(
                name: "zalopay_app_trans_id",
                table: "payments",
                newName: "momo_order_id");
        }
    }
}
