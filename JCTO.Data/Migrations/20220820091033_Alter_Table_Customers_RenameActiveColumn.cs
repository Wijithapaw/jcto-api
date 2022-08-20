using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JCTO.Data.Migrations
{
    public partial class Alter_Table_Customers_RenameActiveColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Active",
                table: "Customers",
                newName: "Inactive");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Inactive",
                table: "Customers",
                newName: "Active");
        }
    }
}
