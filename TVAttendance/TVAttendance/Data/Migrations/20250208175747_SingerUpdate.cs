using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVAttendance.Data.Migrations
{
    /// <inheritdoc />
    public partial class SingerUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Singers");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Singers",
                type: "TEXT",
                maxLength: 35,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Singers",
                type: "TEXT",
                maxLength: 6,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Province",
                table: "Singers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "Singers",
                type: "TEXT",
                maxLength: 60,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "Singers");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Singers");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "Singers");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "Singers");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Singers",
                type: "TEXT",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }
    }
}
