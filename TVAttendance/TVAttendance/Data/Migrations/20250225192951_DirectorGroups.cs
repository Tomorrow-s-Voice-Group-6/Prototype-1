using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVAttendance.Data.Migrations
{
    /// <inheritdoc />
    public partial class DirectorGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chapters_Directors_DirectorID1",
                table: "Chapters");

            migrationBuilder.DropForeignKey(
                name: "FK_Directors_Chapters_ChapterID",
                table: "Directors");

            migrationBuilder.DropIndex(
                name: "IX_Directors_ChapterID",
                table: "Directors");

            migrationBuilder.DropIndex(
                name: "IX_Chapters_DirectorID1",
                table: "Chapters");

            migrationBuilder.DropColumn(
                name: "ChapterID",
                table: "Directors");

            migrationBuilder.DropColumn(
                name: "DirectorID1",
                table: "Chapters");

            migrationBuilder.CreateTable(
                name: "ChapterDirector",
                columns: table => new
                {
                    ChaptersID = table.Column<int>(type: "INTEGER", nullable: false),
                    DirectorsID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChapterDirector", x => new { x.ChaptersID, x.DirectorsID });
                    table.ForeignKey(
                        name: "FK_ChapterDirector_Chapters_ChaptersID",
                        column: x => x.ChaptersID,
                        principalTable: "Chapters",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChapterDirector_Directors_DirectorsID",
                        column: x => x.DirectorsID,
                        principalTable: "Directors",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChapterDirector_DirectorsID",
                table: "ChapterDirector",
                column: "DirectorsID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChapterDirector");

            migrationBuilder.AddColumn<int>(
                name: "ChapterID",
                table: "Directors",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DirectorID1",
                table: "Chapters",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Directors_ChapterID",
                table: "Directors",
                column: "ChapterID");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Directors_Chapters_ChapterID",
                table: "Directors",
                column: "ChapterID",
                principalTable: "Chapters",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
