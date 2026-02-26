using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Event_Management_System.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedEventCenterEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ProviderReference",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerDay",
                table: "EventCentres",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "OrganizerRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReviewedByAdminId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AdminNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizerRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizerRequests_AspNetUsers_ReviewedByAdminId",
                        column: x => x.ReviewedByAdminId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganizerRequests_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizerRequests_ReviewedByAdminId",
                table: "OrganizerRequests",
                column: "ReviewedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizerRequests_UserId",
                table: "OrganizerRequests",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganizerRequests");

            migrationBuilder.DropColumn(
                name: "PricePerDay",
                table: "EventCentres");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderReference",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
