using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DochubSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class _9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscriptionTransactions");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Appointments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Appointments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "SubscriptionTransactions",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentGatewayResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentGatewayTransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionTransactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_SubscriptionTransactions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubscriptionTransactions_UserSubscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "UserSubscriptions",
                        principalColumn: "SubscriptionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionTransaction_CreatedAt",
                table: "SubscriptionTransactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionTransaction_Status",
                table: "SubscriptionTransactions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionTransactions_SubscriptionId",
                table: "SubscriptionTransactions",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionTransactions_UserId",
                table: "SubscriptionTransactions",
                column: "UserId");
        }
    }
}
