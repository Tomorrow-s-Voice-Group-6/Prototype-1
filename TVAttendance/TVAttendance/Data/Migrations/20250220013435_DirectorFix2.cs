using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVAttendance.Data.Migrations
{
    /// <inheritdoc />
    public partial class DirectorFix2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChapterID",
                table: "Directors",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Directors_ChapterID",
                table: "Directors",
                column: "ChapterID");

            migrationBuilder.AddForeignKey(
                name: "FK_Directors_Chapters_ChapterID",
                table: "Directors",
                column: "ChapterID",
                principalTable: "Chapters",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Directors_Chapters_ChapterID",
                table: "Directors");

            migrationBuilder.DropIndex(
                name: "IX_Directors_ChapterID",
                table: "Directors");

            migrationBuilder.DropColumn(
                name: "ChapterID",
                table: "Directors");
        }
    }
}
