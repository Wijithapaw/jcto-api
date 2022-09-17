using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JCTO.Data.Migrations
{
    public partial class Alter_Table_Entries_AddedCol_Index : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "EntryIndex");

            migrationBuilder.AddColumn<int>(
                name: "Index",
                table: "Entries",
                type: "integer",
                nullable: false,
                defaultValueSql: "nextval('\"EntryIndex\"')");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_Index",
                table: "Entries",
                column: "Index",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Entries_Index",
                table: "Entries");

            migrationBuilder.DropSequence(
                name: "EntryIndex");

            migrationBuilder.DropColumn(
                name: "Index",
                table: "Entries");
        }
    }
}
