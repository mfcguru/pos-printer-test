using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POSPrinterTest.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedEpsonPrinter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [Printers]");

            migrationBuilder.InsertData(
                table: "Printers",
                columns: new[] { "Id", "ConnectionString", "ConnectionType", "Name" },
                values: new object[] { 1, "EPSON TM-T82", "USB", "Epson TM-T82" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Printers",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
