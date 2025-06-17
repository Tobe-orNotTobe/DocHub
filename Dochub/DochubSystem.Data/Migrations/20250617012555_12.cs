using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DochubSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class _12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Notifications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ActionUrl",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AppointmentId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DoctorId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "Notifications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadAt",
                table: "Notifications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelatedEntityId",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelatedEntityType",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Notifications",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Notifications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "NotificationTemplates",
                columns: table => new
                {
                    TemplateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EmailBody = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NotificationBody = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TargetRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RequiresEmail = table.Column<bool>(type: "bit", nullable: false),
                    RequiresInApp = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplates", x => x.TemplateId);
                });

            migrationBuilder.CreateTable(
                name: "NotificationHistories",
                columns: table => new
                {
                    HistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TemplateId = table.Column<int>(type: "int", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NotificationBody = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NotificationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DeliveryMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AppointmentId = table.Column<int>(type: "int", nullable: true),
                    DoctorId = table.Column<int>(type: "int", nullable: true),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelatedEntityId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationHistories", x => x.HistoryId);
                    table.ForeignKey(
                        name: "FK_NotificationHistories_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotificationHistories_NotificationTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "NotificationTemplates",
                        principalColumn: "TemplateId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "NotificationQueues",
                columns: table => new
                {
                    QueueId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NotificationBody = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NotificationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetaData = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationQueues", x => x.QueueId);
                    table.ForeignKey(
                        name: "FK_NotificationQueues_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotificationQueues_NotificationTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "NotificationTemplates",
                        principalColumn: "TemplateId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notification_Status",
                table: "Notifications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_Type",
                table: "Notifications",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_User_CreatedAt",
                table: "Notifications",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Notification_User_Status",
                table: "Notifications",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_AppointmentId",
                table: "Notifications",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_DoctorId",
                table: "Notifications",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationHistories_TemplateId",
                table: "NotificationHistories",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationHistory_SentAt",
                table: "NotificationHistories",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationHistory_Type",
                table: "NotificationHistories",
                column: "NotificationType");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationHistory_User_SentAt",
                table: "NotificationHistories",
                columns: new[] { "UserId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationQueue_ScheduledAt",
                table: "NotificationQueues",
                column: "ScheduledAt");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationQueue_Status",
                table: "NotificationQueues",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationQueue_Status_ScheduledAt",
                table: "NotificationQueues",
                columns: new[] { "Status", "ScheduledAt" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationQueues_TemplateId",
                table: "NotificationQueues",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationQueues_UserId",
                table: "NotificationQueues",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplate_TargetRole",
                table: "NotificationTemplates",
                column: "TargetRole");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplate_Type",
                table: "NotificationTemplates",
                column: "Type");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Appointments_AppointmentId",
                table: "Notifications",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "AppointmentId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Doctors_DoctorId",
                table: "Notifications",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "DoctorId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Appointments_AppointmentId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Doctors_DoctorId",
                table: "Notifications");

            migrationBuilder.DropTable(
                name: "NotificationHistories");

            migrationBuilder.DropTable(
                name: "NotificationQueues");

            migrationBuilder.DropTable(
                name: "NotificationTemplates");

            migrationBuilder.DropIndex(
                name: "IX_Notification_Status",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notification_Type",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notification_User_CreatedAt",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notification_User_Status",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_AppointmentId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_DoctorId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ActionUrl",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "AppointmentId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "DoctorId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ReadAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "RelatedEntityId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "RelatedEntityType",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Notifications");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");
        }
    }
}
