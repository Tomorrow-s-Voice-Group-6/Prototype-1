using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVAttendance.Data.Migrations
{
    /// <inheritdoc />
    public partial class DropChapter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Volunteers_Chapters_ChapterID",
                table: "Volunteers");

            migrationBuilder.AlterColumn<int>(
                name: "ChapterID",
                table: "Volunteers",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_Volunteers_Chapters_ChapterID",
                table: "Volunteers",
                column: "ChapterID",
                principalTable: "Chapters",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Volunteers_Chapters_ChapterID",
                table: "Volunteers");

            migrationBuilder.AlterColumn<int>(
                name: "ChapterID",
                table: "Volunteers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Volunteers_Chapters_ChapterID",
                table: "Volunteers",
                column: "ChapterID",
                principalTable: "Chapters",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
