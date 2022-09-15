using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JCTO.Data.Migrations
{
    public partial class Alter_Table_Entry_AddColStockTransactionId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockTransactions_Entries_EntryId",
                table: "StockTransactions");

            migrationBuilder.DropIndex(
                name: "IX_StockTransactions_EntryId",
                table: "StockTransactions");

            migrationBuilder.DropColumn(
                name: "EntryId",
                table: "StockTransactions");

            migrationBuilder.AddColumn<Guid>(
                name: "StockTransactionId",
                table: "Entries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Entries_StockTransactionId",
                table: "Entries",
                column: "StockTransactionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Entries_StockTransactions_StockTransactionId",
                table: "Entries",
                column: "StockTransactionId",
                principalTable: "StockTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entries_StockTransactions_StockTransactionId",
                table: "Entries");

            migrationBuilder.DropIndex(
                name: "IX_Entries_StockTransactionId",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "StockTransactionId",
                table: "Entries");

            migrationBuilder.AddColumn<Guid>(
                name: "EntryId",
                table: "StockTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_EntryId",
                table: "StockTransactions",
                column: "EntryId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransactions_Entries_EntryId",
                table: "StockTransactions",
                column: "EntryId",
                principalTable: "Entries",
                principalColumn: "Id");
        }
    }
}
