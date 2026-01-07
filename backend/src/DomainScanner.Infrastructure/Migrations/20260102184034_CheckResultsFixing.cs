using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DomainScanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CheckResultsFixing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CheckResults_Users_UserId",
                table: "CheckResults");

            migrationBuilder.DropIndex(
                name: "IX_CheckResults_UserId",
                table: "CheckResults");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CheckResults");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "CheckResults",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_CheckResults_UserId",
                table: "CheckResults",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CheckResults_Users_UserId",
                table: "CheckResults",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
