using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVAttendance.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAddressField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Chapters",
                newName: "Street");

            migrationBuilder.AddColumn<int>(
                name: "Province",
                table: "Chapters",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Chapters",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "Chapters",
                type: "TEXT",
                maxLength: 6,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Province",
                table: "Chapters");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Chapters");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "Chapters");

            migrationBuilder.RenameColumn(
                name: "Street",
                table: "Chapters",
                newName: "Address");
        }
    }
}
