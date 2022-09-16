using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JCTO.Data.Migrations
{
    public partial class Alter_Table_EntryTransactions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ApprovalType",
                table: "EntryTransactions",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovalTransactionId",
                table: "EntryTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntryTransactions_ApprovalTransactionId",
                table: "EntryTransactions",
                column: "ApprovalTransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_EntryTransactions_EntryTransactions_ApprovalTransactionId",
                table: "EntryTransactions",
                column: "ApprovalTransactionId",
                principalTable: "EntryTransactions",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EntryTransactions_EntryTransactions_ApprovalTransactionId",
                table: "EntryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_EntryTransactions_ApprovalTransactionId",
                table: "EntryTransactions");

            migrationBuilder.DropColumn(
                name: "ApprovalTransactionId",
                table: "EntryTransactions");

            migrationBuilder.AlterColumn<int>(
                name: "ApprovalType",
                table: "EntryTransactions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
