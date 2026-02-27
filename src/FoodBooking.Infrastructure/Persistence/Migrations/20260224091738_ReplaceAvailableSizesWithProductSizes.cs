using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FoodBooking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceAvailableSizesWithProductSizes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "available_sizes",
                table: "products");

            migrationBuilder.CreateTable(
                name: "product_sizes",
                columns: table => new
                {
                    product_size_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    size = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    stock_quantity = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_sizes", x => x.product_size_id);
                    table.ForeignKey(
                        name: "FK_product_sizes_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_product_sizes_product_id",
                table: "product_sizes",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_sizes_product_id_size",
                table: "product_sizes",
                columns: new[] { "product_id", "size" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_sizes");

            migrationBuilder.AddColumn<string>(
                name: "available_sizes",
                table: "products",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");
        }
    }
}
