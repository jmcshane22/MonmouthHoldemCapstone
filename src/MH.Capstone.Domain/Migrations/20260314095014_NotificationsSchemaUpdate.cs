using MH.Capstone.Domain.DataModels;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace MH.Capstone.Domain.Migrations
{
    /// <inheritdoc />
    public partial class NotificationsSchemaUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailNotification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeliveredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastFailure = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    FailureCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailNotification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailNotification_Notification_Id",
                        column: x => x.Id,
                        principalTable: "Notification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InAppNotification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InAppNotification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InAppNotification_Notification_Id",
                        column: x => x.Id,
                        principalTable: "Notification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Manually added SQL to migrate existing data from Notification to InAppNotification
            migrationBuilder.Sql("INSERT INTO [dbo].[InAppNotification] ([Id], [IsRead]) " +
                                 "SELECT [base].[Id], [base].[IsRead] FROM [dbo].[Notification] AS [base]");

            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "Notification");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "Notification",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Manually added SQL to migrate existing data from InAppNotification to Notification
            migrationBuilder.Sql("UPDATE [dbo].[Notification] " +
                                 "SET [base].[IsRead] = [inApp].[IsRead] " +
                                 "FROM [dbo].[Notification] AS [base] " +
                                 "JOIN [dbo].[InAppNotification] AS [inApp] " +
                                 "ON [base].[ID] = [inApp].[ID]");

            migrationBuilder.DropTable(
                name: "EmailNotification");

            migrationBuilder.DropTable(
                name: "InAppNotification");
        }
    }
}
