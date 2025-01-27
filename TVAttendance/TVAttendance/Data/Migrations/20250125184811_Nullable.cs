using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVAttendance.Data.Migrations
{
    /// <inheritdoc />
    public partial class Nullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chapters_Directors_DirectorID",
                table: "Chapters");

            migrationBuilder.AlterColumn<int>(
                name: "DirectorID",
                table: "Chapters",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Chapters_Directors_DirectorID",
                table: "Chapters",
                column: "DirectorID",
                principalTable: "Directors",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chapters_Directors_DirectorID",
                table: "Chapters");

            migrationBuilder.AlterColumn<int>(
                name: "DirectorID",
                table: "Chapters",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_Chapters_Directors_DirectorID",
                table: "Chapters",
                column: "DirectorID",
                principalTable: "Directors",
                principalColumn: "ID");
        }
    }
}
