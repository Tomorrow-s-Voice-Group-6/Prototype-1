using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVAttendance.Data.Migrations
{
    /// <inheritdoc />
    public partial class CitiesAndSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SingerSessions_Sessions_ProgramID",
                table: "SingerSessions");

            migrationBuilder.RenameColumn(
                name: "ProgramID",
                table: "SingerSessions",
                newName: "SessionID");

            migrationBuilder.RenameIndex(
                name: "IX_SingerSessions_ProgramID",
                table: "SingerSessions",
                newName: "IX_SingerSessions_SessionID");

            migrationBuilder.AddColumn<int>(
                name: "CityID",
                table: "Singers",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CityID",
                table: "Sessions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CityID",
                table: "Chapters",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    CityID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CityName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.CityID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Singers_CityID",
                table: "Singers",
                column: "CityID");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_CityID",
                table: "Sessions",
                column: "CityID");

            migrationBuilder.CreateIndex(
                name: "IX_Chapters_CityID",
                table: "Chapters",
                column: "CityID");

            migrationBuilder.AddForeignKey(
                name: "FK_Chapters_Cities_CityID",
                table: "Chapters",
                column: "CityID",
                principalTable: "Cities",
                principalColumn: "CityID");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Cities_CityID",
                table: "Sessions",
                column: "CityID",
                principalTable: "Cities",
                principalColumn: "CityID");

            migrationBuilder.AddForeignKey(
                name: "FK_Singers_Cities_CityID",
                table: "Singers",
                column: "CityID",
                principalTable: "Cities",
                principalColumn: "CityID");

            migrationBuilder.AddForeignKey(
                name: "FK_SingerSessions_Sessions_SessionID",
                table: "SingerSessions",
                column: "SessionID",
                principalTable: "Sessions",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chapters_Cities_CityID",
                table: "Chapters");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Cities_CityID",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Singers_Cities_CityID",
                table: "Singers");

            migrationBuilder.DropForeignKey(
                name: "FK_SingerSessions_Sessions_SessionID",
                table: "SingerSessions");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropIndex(
                name: "IX_Singers_CityID",
                table: "Singers");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_CityID",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Chapters_CityID",
                table: "Chapters");

            migrationBuilder.DropColumn(
                name: "CityID",
                table: "Singers");

            migrationBuilder.DropColumn(
                name: "CityID",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "CityID",
                table: "Chapters");

            migrationBuilder.RenameColumn(
                name: "SessionID",
                table: "SingerSessions",
                newName: "ProgramID");

            migrationBuilder.RenameIndex(
                name: "IX_SingerSessions_SessionID",
                table: "SingerSessions",
                newName: "IX_SingerSessions_ProgramID");

            migrationBuilder.AddForeignKey(
                name: "FK_SingerSessions_Sessions_ProgramID",
                table: "SingerSessions",
                column: "ProgramID",
                principalTable: "Sessions",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
