using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JCTO.Data.Migrations
{
    public partial class Alter_Table_BowserEntries_Renamed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BowserEntry_Orders_OrderId",
                table: "BowserEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_BowserEntry_Users_CreatedById",
                table: "BowserEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_BowserEntry_Users_LastUpdatedById",
                table: "BowserEntry");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BowserEntry",
                table: "BowserEntry");

            migrationBuilder.RenameTable(
                name: "BowserEntry",
                newName: "BowserEntries");

            migrationBuilder.RenameIndex(
                name: "IX_BowserEntry_OrderId",
                table: "BowserEntries",
                newName: "IX_BowserEntries_OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_BowserEntry_LastUpdatedById",
                table: "BowserEntries",
                newName: "IX_BowserEntries_LastUpdatedById");

            migrationBuilder.RenameIndex(
                name: "IX_BowserEntry_CreatedById",
                table: "BowserEntries",
                newName: "IX_BowserEntries_CreatedById");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BowserEntries",
                table: "BowserEntries",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BowserEntries_Orders_OrderId",
                table: "BowserEntries",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BowserEntries_Users_CreatedById",
                table: "BowserEntries",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BowserEntries_Users_LastUpdatedById",
                table: "BowserEntries",
                column: "LastUpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BowserEntries_Orders_OrderId",
                table: "BowserEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_BowserEntries_Users_CreatedById",
                table: "BowserEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_BowserEntries_Users_LastUpdatedById",
                table: "BowserEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BowserEntries",
                table: "BowserEntries");

            migrationBuilder.RenameTable(
                name: "BowserEntries",
                newName: "BowserEntry");

            migrationBuilder.RenameIndex(
                name: "IX_BowserEntries_OrderId",
                table: "BowserEntry",
                newName: "IX_BowserEntry_OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_BowserEntries_LastUpdatedById",
                table: "BowserEntry",
                newName: "IX_BowserEntry_LastUpdatedById");

            migrationBuilder.RenameIndex(
                name: "IX_BowserEntries_CreatedById",
                table: "BowserEntry",
                newName: "IX_BowserEntry_CreatedById");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BowserEntry",
                table: "BowserEntry",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BowserEntry_Orders_OrderId",
                table: "BowserEntry",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BowserEntry_Users_CreatedById",
                table: "BowserEntry",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BowserEntry_Users_LastUpdatedById",
                table: "BowserEntry",
                column: "LastUpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
