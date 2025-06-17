using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DochubSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class _4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Appointments");

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    PlanId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MonthlyPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    YearlyPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ConsultationsPerMonth = table.Column<int>(type: "int", nullable: false),
                    MaxDoctorsAccess = table.Column<int>(type: "int", nullable: false),
                    HasVideoCallSupport = table.Column<bool>(type: "bit", nullable: false),
                    HasPriorityBooking = table.Column<bool>(type: "bit", nullable: false),
                    Has24x7Support = table.Column<bool>(type: "bit", nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    HasMedicationReminders = table.Column<bool>(type: "bit", nullable: false),
                    HasHealthReports = table.Column<bool>(type: "bit", nullable: false),
                    HasBasicMedicalInfo = table.Column<bool>(type: "bit", nullable: false),
                    HasMedicalRecordStorage = table.Column<bool>(type: "bit", nullable: false),
                    HasExamReminders = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.PlanId);
                });

            migrationBuilder.CreateTable(
                name: "UserSubscriptions",
                columns: table => new
                {
                    SubscriptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BillingCycle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PendingPlanId = table.Column<int>(type: "int", nullable: true),
                    PlanChangeEffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConsultationsUsed = table.Column<int>(type: "int", nullable: false),
                    LastConsultationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubscriptions", x => x.SubscriptionId);
                    table.ForeignKey(
                        name: "FK_UserSubscriptions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserSubscriptions_SubscriptionPlans_PendingPlanId",
                        column: x => x.PendingPlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "PlanId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserSubscriptions_SubscriptionPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "PlanId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConsultationUsages",
                columns: table => new
                {
                    UsageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    AppointmentId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UsageDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConsultationType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationUsages", x => x.UsageId);
                    table.ForeignKey(
                        name: "FK_ConsultationUsages_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "AppointmentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsultationUsages_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsultationUsages_UserSubscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "UserSubscriptions",
                        principalColumn: "SubscriptionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionTransactions",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PaymentGatewayTransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentGatewayResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                name: "IX_ConsultationUsage_SubscriptionId",
                table: "ConsultationUsages",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationUsage_User_Date",
                table: "ConsultationUsages",
                columns: new[] { "UserId", "UsageDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationUsages_AppointmentId",
                table: "ConsultationUsages",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlan_Name",
                table: "SubscriptionPlans",
                column: "Name",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscription_EndDate",
                table: "UserSubscriptions",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscription_Status",
                table: "UserSubscriptions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscription_UserId",
                table: "UserSubscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_PendingPlanId",
                table: "UserSubscriptions",
                column: "PendingPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_PlanId",
                table: "UserSubscriptions",
                column: "PlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsultationUsages");

            migrationBuilder.DropTable(
                name: "SubscriptionTransactions");

            migrationBuilder.DropTable(
                name: "UserSubscriptions");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans");

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppointmentId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payments_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "AppointmentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_AppointmentId",
                table: "Payments",
                column: "AppointmentId");
        }
    }
}
