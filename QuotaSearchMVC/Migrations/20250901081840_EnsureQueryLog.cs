using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuotaSearchMVC.Migrations
{
    /// <inheritdoc />
    public partial class EnsureQueryLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Term",
                table: "QueryLogs",
                newName: "QueryText");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "QueryText",
                table: "QueryLogs",
                newName: "Term");
        }
    }
}
