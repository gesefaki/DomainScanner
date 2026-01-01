using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DomainScanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class init1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CheckResults_Domains_DomainEntityId",
                table: "CheckResults");

            migrationBuilder.DropIndex(
                name: "IX_CheckResults_DomainEntityId",
                table: "CheckResults");

            migrationBuilder.DropColumn(
                name: "DomainEntityId",
                table: "CheckResults");

            migrationBuilder.CreateIndex(
                name: "IX_CheckResults_DomainId",
                table: "CheckResults",
                column: "DomainId");

            migrationBuilder.AddForeignKey(
                name: "FK_CheckResults_Domains_DomainId",
                table: "CheckResults",
                column: "DomainId",
                principalTable: "Domains",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CheckResults_Domains_DomainId",
                table: "CheckResults");

            migrationBuilder.DropIndex(
                name: "IX_CheckResults_DomainId",
                table: "CheckResults");

            migrationBuilder.AddColumn<Guid>(
                name: "DomainEntityId",
                table: "CheckResults",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CheckResults_DomainEntityId",
                table: "CheckResults",
                column: "DomainEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_CheckResults_Domains_DomainEntityId",
                table: "CheckResults",
                column: "DomainEntityId",
                principalTable: "Domains",
                principalColumn: "Id");
        }
    }
}
