using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DomainScanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMonitoringAndRetentionIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MonitoringEnabled",
                table: "Domains",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_Domains_MonitoringEnabled_UpdatedAt",
                table: "Domains",
                columns: new[] { "MonitoringEnabled", "UpdatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Domains_MonitoringEnabled_UpdatedAt",
                table: "Domains");

            migrationBuilder.DropColumn(
                name: "MonitoringEnabled",
                table: "Domains");
        }
    }
}
