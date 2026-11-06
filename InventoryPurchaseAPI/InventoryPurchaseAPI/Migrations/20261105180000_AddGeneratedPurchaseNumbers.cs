using InventoryPurchaseAPI.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryPurchaseAPI.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261105180000_AddGeneratedPurchaseNumbers")]
    public partial class AddGeneratedPurchaseNumbers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE SEQUENCE IF NOT EXISTS invoice_number_seq;
                CREATE SEQUENCE IF NOT EXISTS lot_number_seq;

                SELECT setval(
                    'invoice_number_seq',
                    COALESCE(
                        (SELECT MAX(NULLIF(SUBSTRING("InvoiceNumber" FROM '^INV-([0-9]+)$'), '')::bigint)
                         FROM "Purchases"),
                        0
                    ) + 1,
                    false
                );

                SELECT setval(
                    'lot_number_seq',
                    COALESCE(
                        (SELECT MAX(NULLIF(SUBSTRING("LotNumber" FROM '^LOT-([0-9]+)$'), '')::bigint)
                         FROM "PurchaseItems"),
                        0
                    ) + 1,
                    false
                );

                WITH ranked_invoices AS
                (
                    SELECT
                        "Id",
                        "InvoiceNumber",
                        ROW_NUMBER() OVER (PARTITION BY "InvoiceNumber" ORDER BY "Id") AS row_number
                    FROM "Purchases"
                ),
                replacement_invoices AS
                (
                    SELECT
                        "Id",
                        'INV-' || LPAD(nextval('invoice_number_seq')::text, 4, '0') AS "InvoiceNumber"
                    FROM ranked_invoices
                    WHERE BTRIM("InvoiceNumber") = '' OR row_number > 1
                )
                UPDATE "Purchases" AS purchases
                SET "InvoiceNumber" = replacements."InvoiceNumber"
                FROM replacement_invoices AS replacements
                WHERE purchases."Id" = replacements."Id";

                WITH ranked_lots AS
                (
                    SELECT
                        "Id",
                        "LotNumber",
                        ROW_NUMBER() OVER (PARTITION BY "LotNumber" ORDER BY "Id") AS row_number
                    FROM "PurchaseItems"
                ),
                replacement_lots AS
                (
                    SELECT
                        "Id",
                        'LOT-' || LPAD(nextval('lot_number_seq')::text, 4, '0') AS "LotNumber"
                    FROM ranked_lots
                    WHERE BTRIM("LotNumber") = '' OR row_number > 1
                )
                UPDATE "PurchaseItems" AS purchase_items
                SET "LotNumber" = replacements."LotNumber"
                FROM replacement_lots AS replacements
                WHERE purchase_items."Id" = replacements."Id";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_InvoiceNumber",
                table: "Purchases",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseItems_LotNumber",
                table: "PurchaseItems",
                column: "LotNumber",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Purchases_InvoiceNumber",
                table: "Purchases");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseItems_LotNumber",
                table: "PurchaseItems");

            migrationBuilder.Sql("""
                DROP SEQUENCE IF EXISTS invoice_number_seq;
                DROP SEQUENCE IF EXISTS lot_number_seq;
                """);
        }
    }
}
