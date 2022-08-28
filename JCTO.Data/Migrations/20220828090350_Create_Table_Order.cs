using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JCTO.Data.Migrations
{
    public partial class Create_Table_Order : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EntryTransaction_Entries_EntryId",
                table: "EntryTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_EntryTransaction_Users_CreatedById",
                table: "EntryTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_EntryTransaction_Users_LastUpdatedById",
                table: "EntryTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EntryTransaction",
                table: "EntryTransaction");

            migrationBuilder.RenameTable(
                name: "EntryTransaction",
                newName: "EntryTransactions");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "EntryTransactions",
                newName: "Quantity");

            migrationBuilder.RenameIndex(
                name: "IX_EntryTransaction_LastUpdatedById",
                table: "EntryTransactions",
                newName: "IX_EntryTransactions_LastUpdatedById");

            migrationBuilder.RenameIndex(
                name: "IX_EntryTransaction_EntryId_Type",
                table: "EntryTransactions",
                newName: "IX_EntryTransactions_EntryId_Type");

            migrationBuilder.RenameIndex(
                name: "IX_EntryTransaction_CreatedById",
                table: "EntryTransactions",
                newName: "IX_EntryTransactions_CreatedById");

            migrationBuilder.AddColumn<double>(
                name: "RemainingQuantity",
                table: "Entries",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DeliveredQuantity",
                table: "EntryTransactions",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "ObRef",
                table: "EntryTransactions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "OrderId",
                table: "EntryTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_EntryTransactions",
                table: "EntryTransactions",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OrderDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Buyer = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<double>(type: "double precision", nullable: false),
                    ObRefPrefix = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TankNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BuyerType = table.Column<int>(type: "integer", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    LastUpdatedDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConcurrencyKey = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Users_LastUpdatedById",
                        column: x => x.LastUpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BowserEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Capacity = table.Column<double>(type: "double precision", nullable: false),
                    Count = table.Column<double>(type: "double precision", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    LastUpdatedDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConcurrencyKey = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BowserEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BowserEntry_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BowserEntry_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BowserEntry_Users_LastUpdatedById",
                        column: x => x.LastUpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntryTransactions_OrderId",
                table: "EntryTransactions",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_BowserEntry_CreatedById",
                table: "BowserEntry",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_BowserEntry_LastUpdatedById",
                table: "BowserEntry",
                column: "LastUpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_BowserEntry_OrderId",
                table: "BowserEntry",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedById",
                table: "Orders",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_LastUpdatedById",
                table: "Orders",
                column: "LastUpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderDate_OrderNo",
                table: "Orders",
                columns: new[] { "OrderDate", "OrderNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ProductId",
                table: "Orders",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_EntryTransactions_Entries_EntryId",
                table: "EntryTransactions",
                column: "EntryId",
                principalTable: "Entries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EntryTransactions_Orders_OrderId",
                table: "EntryTransactions",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EntryTransactions_Users_CreatedById",
                table: "EntryTransactions",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EntryTransactions_Users_LastUpdatedById",
                table: "EntryTransactions",
                column: "LastUpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EntryTransactions_Entries_EntryId",
                table: "EntryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_EntryTransactions_Orders_OrderId",
                table: "EntryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_EntryTransactions_Users_CreatedById",
                table: "EntryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_EntryTransactions_Users_LastUpdatedById",
                table: "EntryTransactions");

            migrationBuilder.DropTable(
                name: "BowserEntry");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EntryTransactions",
                table: "EntryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_EntryTransactions_OrderId",
                table: "EntryTransactions");

            migrationBuilder.DropColumn(
                name: "RemainingQuantity",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "DeliveredQuantity",
                table: "EntryTransactions");

            migrationBuilder.DropColumn(
                name: "ObRef",
                table: "EntryTransactions");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "EntryTransactions");

            migrationBuilder.RenameTable(
                name: "EntryTransactions",
                newName: "EntryTransaction");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "EntryTransaction",
                newName: "Amount");

            migrationBuilder.RenameIndex(
                name: "IX_EntryTransactions_LastUpdatedById",
                table: "EntryTransaction",
                newName: "IX_EntryTransaction_LastUpdatedById");

            migrationBuilder.RenameIndex(
                name: "IX_EntryTransactions_EntryId_Type",
                table: "EntryTransaction",
                newName: "IX_EntryTransaction_EntryId_Type");

            migrationBuilder.RenameIndex(
                name: "IX_EntryTransactions_CreatedById",
                table: "EntryTransaction",
                newName: "IX_EntryTransaction_CreatedById");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EntryTransaction",
                table: "EntryTransaction",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EntryTransaction_Entries_EntryId",
                table: "EntryTransaction",
                column: "EntryId",
                principalTable: "Entries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EntryTransaction_Users_CreatedById",
                table: "EntryTransaction",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EntryTransaction_Users_LastUpdatedById",
                table: "EntryTransaction",
                column: "LastUpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
