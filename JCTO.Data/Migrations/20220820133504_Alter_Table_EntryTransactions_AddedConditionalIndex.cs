using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JCTO.Data.Migrations
{
    public partial class Alter_Table_EntryTransactions_AddedConditionalIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EntryTransaction_EntryId",
                table: "EntryTransaction");

            migrationBuilder.CreateIndex(
                name: "IX_EntryTransaction_EntryId_Type",
                table: "EntryTransaction",
                columns: new[] { "EntryId", "Type" },
                filter: "\"Type\" = 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EntryTransaction_EntryId_Type",
                table: "EntryTransaction");

            migrationBuilder.CreateIndex(
                name: "IX_EntryTransaction_EntryId",
                table: "EntryTransaction",
                column: "EntryId");
        }
    }
}
