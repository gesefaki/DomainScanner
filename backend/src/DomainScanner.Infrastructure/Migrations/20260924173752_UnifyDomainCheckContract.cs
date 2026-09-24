using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DomainScanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UnifyDomainCheckContract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CheckResults_DomainId",
                table: "CheckResults");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "CheckResults",
                newName: "RequestedAddress");

            migrationBuilder.AlterColumn<int>(
                name: "StatusCode",
                table: "CheckResults",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "ErrorCode",
                table: "CheckResults",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinalAddress",
                table: "CheckResults",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Kind",
                table: "CheckResults",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "http");

            migrationBuilder.AddColumn<string>(
                name: "Outcome",
                table: "CheckResults",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string[]>(
                name: "Redirects",
                table: "CheckResults",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<long>(
                name: "ResponseTimeMs",
                table: "CheckResults",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "TlsCertificateExpiresAt",
                table: "CheckResults",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TlsHasValidationErrors",
                table: "CheckResults",
                type: "boolean",
                nullable: true);

            // The old Address column represented the final URL. The original
            // request URL and the cause of historical 502/504 results were not
            // stored, so those details cannot be reconstructed.
            migrationBuilder.Sql("""
                UPDATE "CheckResults"
                SET "FinalAddress" = "RequestedAddress",
                    "Outcome" = CASE WHEN "IsActive" THEN 'up' ELSE 'down' END;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_CheckResults_DomainId_CreatedAt_Id",
                table: "CheckResults",
                columns: new[] { "DomainId", "CreatedAt", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CheckResults_DomainId_CreatedAt_Id",
                table: "CheckResults");

            migrationBuilder.DropColumn(
                name: "ErrorCode",
                table: "CheckResults");

            migrationBuilder.DropColumn(
                name: "FinalAddress",
                table: "CheckResults");

            migrationBuilder.DropColumn(
                name: "Kind",
                table: "CheckResults");

            migrationBuilder.DropColumn(
                name: "Outcome",
                table: "CheckResults");

            migrationBuilder.DropColumn(
                name: "Redirects",
                table: "CheckResults");

            migrationBuilder.DropColumn(
                name: "ResponseTimeMs",
                table: "CheckResults");

            migrationBuilder.DropColumn(
                name: "TlsCertificateExpiresAt",
                table: "CheckResults");

            migrationBuilder.DropColumn(
                name: "TlsHasValidationErrors",
                table: "CheckResults");

            migrationBuilder.RenameColumn(
                name: "RequestedAddress",
                table: "CheckResults",
                newName: "Address");

            // Older schema cannot represent a transport failure without an
            // HTTP status. Use zero for those rows so the column can be made
            // non-nullable during a rollback.
            migrationBuilder.Sql("""
                UPDATE "CheckResults"
                SET "StatusCode" = 0
                WHERE "StatusCode" IS NULL;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "StatusCode",
                table: "CheckResults",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CheckResults_DomainId",
                table: "CheckResults",
                column: "DomainId");
        }
    }
}
