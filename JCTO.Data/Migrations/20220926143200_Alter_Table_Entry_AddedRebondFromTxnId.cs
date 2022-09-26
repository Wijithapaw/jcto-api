using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JCTO.Data.Migrations
{
    public partial class Alter_Table_Entry_AddedRebondFromTxnId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RebondFromEntryTxnId",
                table: "Entries",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Entries_RebondFromEntryTxnId",
                table: "Entries",
                column: "RebondFromEntryTxnId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Entries_EntryTransactions_RebondFromEntryTxnId",
                table: "Entries",
                column: "RebondFromEntryTxnId",
                principalTable: "EntryTransactions",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entries_EntryTransactions_RebondFromEntryTxnId",
                table: "Entries");

            migrationBuilder.DropIndex(
                name: "IX_Entries_RebondFromEntryTxnId",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "RebondFromEntryTxnId",
                table: "Entries");
        }
    }
}
