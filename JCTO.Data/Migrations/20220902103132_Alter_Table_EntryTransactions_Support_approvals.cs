using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JCTO.Data.Migrations
{
    public partial class Alter_Table_EntryTransactions_Support_approvals : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EntryTransactions_EntryId_Type",
                table: "EntryTransactions");

            migrationBuilder.DropColumn(
                name: "TransactionDateTimeUtc",
                table: "EntryTransactions");

            migrationBuilder.AddColumn<string>(
                name: "ApprovalRef",
                table: "EntryTransactions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovalType",
                table: "EntryTransactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "TransactionDate",
                table: "EntryTransactions",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_EntryTransactions_ApprovalType_ApprovalRef",
                table: "EntryTransactions",
                columns: new[] { "ApprovalType", "ApprovalRef" },
                filter: "\"Type\" = 0 AND \"ApprovalType\" in (1,2)");

            migrationBuilder.CreateIndex(
                name: "IX_EntryTransactions_EntryId",
                table: "EntryTransactions",
                column: "EntryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EntryTransactions_ApprovalType_ApprovalRef",
                table: "EntryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_EntryTransactions_EntryId",
                table: "EntryTransactions");

            migrationBuilder.DropColumn(
                name: "ApprovalRef",
                table: "EntryTransactions");

            migrationBuilder.DropColumn(
                name: "ApprovalType",
                table: "EntryTransactions");

            migrationBuilder.DropColumn(
                name: "TransactionDate",
                table: "EntryTransactions");

            migrationBuilder.AddColumn<DateTime>(
                name: "TransactionDateTimeUtc",
                table: "EntryTransactions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_EntryTransactions_EntryId_Type",
                table: "EntryTransactions",
                columns: new[] { "EntryId", "Type" },
                filter: "\"Type\" = 0");
        }
    }
}
