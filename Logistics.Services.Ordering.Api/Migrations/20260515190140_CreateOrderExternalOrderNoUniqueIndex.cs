using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Services.Ordering.Api.Migrations
{
    /// <inheritdoc />
    public partial class CreateOrderExternalOrderNoUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Orders_TenantId_ExternalOrderNo",
                table: "Orders",
                columns: new[] { "TenantId", "ExternalOrderNo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_TenantId_ExternalOrderNo",
                table: "Orders");
        }
    }
}
