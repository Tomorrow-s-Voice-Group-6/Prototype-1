using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVAttendance.Data.Migrations
{
    /// <inheritdoc />
    public partial class DirectorFix3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chapters_Directors_DirectorID",
                table: "Chapters");

            migrationBuilder.DropIndex(
                name: "IX_Chapters_DirectorID",
                table: "Chapters");

            migrationBuilder.AddColumn<int>(
                name: "DirectorID1",
                table: "Chapters",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chapters_DirectorID1",
                table: "Chapters",
                column: "DirectorID1");

            migrationBuilder.AddForeignKey(
                name: "FK_Chapters_Directors_DirectorID1",
                table: "Chapters",
                column: "DirectorID1",
                principalTable: "Directors",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chapters_Directors_DirectorID1",
                table: "Chapters");

            migrationBuilder.DropIndex(
                name: "IX_Chapters_DirectorID1",
                table: "Chapters");

            migrationBuilder.DropColumn(
                name: "DirectorID1",
                table: "Chapters");

            migrationBuilder.CreateIndex(
                name: "IX_Chapters_DirectorID",
                table: "Chapters",
                column: "DirectorID");

            migrationBuilder.AddForeignKey(
                name: "FK_Chapters_Directors_DirectorID",
                table: "Chapters",
                column: "DirectorID",
                principalTable: "Directors",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
