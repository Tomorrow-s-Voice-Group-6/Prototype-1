using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVAttendance.Migrations
{
    /// <inheritdoc />
    public partial class AllMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Directors",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DOB = table.Column<DateTime>(type: "TEXT", nullable: true),
                    HireDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Status = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Directors", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Chapters",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    DirectorID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chapters", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Chapters_Directors_DirectorID",
                        column: x => x.DirectorID,
                        principalTable: "Directors",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ChapterID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Sessions_Chapters_ChapterID",
                        column: x => x.ChapterID,
                        principalTable: "Chapters",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Singers",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DOB = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Status = table.Column<bool>(type: "INTEGER", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EmergencyContactFirstName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    EmergencyContactLastName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    EmergencyContactPhone = table.Column<string>(type: "TEXT", nullable: false),
                    ChapterID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Singers", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Singers_Chapters_ChapterID",
                        column: x => x.ChapterID,
                        principalTable: "Chapters",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Volunteers",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 55, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    DOB = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    RegisterDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ChapterID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Volunteers", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Volunteers_Chapters_ChapterID",
                        column: x => x.ChapterID,
                        principalTable: "Chapters",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SingerSessions",
                columns: table => new
                {
                    SingerID = table.Column<int>(type: "INTEGER", nullable: false),
                    SessionID = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SingerSessions", x => new { x.SingerID, x.SessionID });
                    table.ForeignKey(
                        name: "FK_SingerSessions_Sessions_SessionID",
                        column: x => x.SessionID,
                        principalTable: "Sessions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SingerSessions_Singers_SingerID",
                        column: x => x.SingerID,
                        principalTable: "Singers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chapters_DirectorID",
                table: "Chapters",
                column: "DirectorID");

            migrationBuilder.CreateIndex(
                name: "IX_Directors_FirstName_LastName_DOB",
                table: "Directors",
                columns: new[] { "FirstName", "LastName", "DOB" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_ChapterID",
                table: "Sessions",
                column: "ChapterID");

            migrationBuilder.CreateIndex(
                name: "IX_Singers_ChapterID",
                table: "Singers",
                column: "ChapterID");

            migrationBuilder.CreateIndex(
                name: "IX_Singers_FirstName_LastName_DOB",
                table: "Singers",
                columns: new[] { "FirstName", "LastName", "DOB" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SingerSessions_SessionID",
                table: "SingerSessions",
                column: "SessionID");

            migrationBuilder.CreateIndex(
                name: "IX_Volunteers_ChapterID",
                table: "Volunteers",
                column: "ChapterID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SingerSessions");

            migrationBuilder.DropTable(
                name: "Volunteers");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "Singers");

            migrationBuilder.DropTable(
                name: "Chapters");

            migrationBuilder.DropTable(
                name: "Directors");
        }
    }
}
