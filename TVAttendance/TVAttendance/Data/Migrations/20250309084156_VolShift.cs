using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVAttendance.Data.Migrations
{
    /// <inheritdoc />
    public partial class VolShift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ClockIn",
                table: "ShiftVolunteers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClockOut",
                table: "ShiftVolunteers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NonAttendance",
                table: "ShiftVolunteers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "ShiftVolunteers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClockIn",
                table: "ShiftVolunteers");

            migrationBuilder.DropColumn(
                name: "ClockOut",
                table: "ShiftVolunteers");

            migrationBuilder.DropColumn(
                name: "NonAttendance",
                table: "ShiftVolunteers");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "ShiftVolunteers");
        }
    }
}
