using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Event_Management_System.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedBookingEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "BookingReservationExpiresAt",
                table: "Bookings",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PaymentCompletedAt",
                table: "Bookings",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentReference",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookingReservationExpiresAt",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PaymentCompletedAt",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PaymentReference",
                table: "Bookings");
        }
    }
}
