using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FoodBooking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrdersTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_wards_code",
                table: "wards",
                column: "code");

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    customer_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    address_detail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ward_code = table.Column<int>(type: "integer", nullable: true),
                    district_code = table.Column<int>(type: "integer", nullable: true),
                    province_code = table.Column<int>(type: "integer", nullable: true),
                    full_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    customer_lat = table.Column<double>(type: "double precision", nullable: true),
                    customer_lng = table.Column<double>(type: "double precision", nullable: true),
                    shop_lat = table.Column<double>(type: "double precision", nullable: false, defaultValue: 10.8506),
                    shop_lng = table.Column<double>(type: "double precision", nullable: false, defaultValue: 106.7749),
                    shipper_id = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "pending"),
                    total_price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.id);
                    table.ForeignKey(
                        name: "FK_orders_districts_district_code",
                        column: x => x.district_code,
                        principalTable: "districts",
                        principalColumn: "code",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_orders_provinces_province_code",
                        column: x => x.province_code,
                        principalTable: "provinces",
                        principalColumn: "code",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_orders_wards_ward_code",
                        column: x => x.ward_code,
                        principalTable: "wards",
                        principalColumn: "code",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    product_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_items_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_order_items_order_id",
                table: "order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_product_id",
                table: "order_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_created_at",
                table: "orders",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_orders_customer_phone",
                table: "orders",
                column: "customer_phone");

            migrationBuilder.CreateIndex(
                name: "IX_orders_district_code",
                table: "orders",
                column: "district_code");

            migrationBuilder.CreateIndex(
                name: "IX_orders_province_code",
                table: "orders",
                column: "province_code");

            migrationBuilder.CreateIndex(
                name: "IX_orders_shipper_id",
                table: "orders",
                column: "shipper_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_status",
                table: "orders",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_orders_ward_code",
                table: "orders",
                column: "ward_code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_wards_code",
                table: "wards");
        }
    }
}
