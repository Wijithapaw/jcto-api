using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JCTO.Data.Migrations
{
    public partial class Alter_Table_Entry_ModifiedOneToOneRelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StockTransactionId",
                table: "Entries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Entries_StockTransactionId",
                table: "Entries",
                column: "StockTransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Entries_StockTransactions_StockTransactionId",
                table: "Entries",
                column: "StockTransactionId",
                principalTable: "StockTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
