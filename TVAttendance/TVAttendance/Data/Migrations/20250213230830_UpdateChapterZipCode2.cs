using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVAttendance.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateChapterZipCode2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ZipCode",
                table: "Chapters",
                type: "TEXT",
                maxLength: 6,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 6);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ZipCode",
                table: "Chapters",
                type: "TEXT",
                maxLength: 6,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 6,
                oldNullable: true);
        }
    }
}
