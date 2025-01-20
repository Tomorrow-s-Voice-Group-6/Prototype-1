using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVAttendance.Data.Migrations
{
    /// <inheritdoc />
    public partial class TVContextConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmploymentDate",
                table: "Volunteers",
                newName: "RegisterDate");

            migrationBuilder.RenameColumn(
                name: "EmploymentDate",
                table: "Directors",
                newName: "HireDate");

            migrationBuilder.AddColumn<int>(
                name: "ChapterID",
                table: "Volunteers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Singers",
                type: "TEXT",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ChapterID",
                table: "Singers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Singers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Chapters",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    DirectorID = table.Column<int>(type: "INTEGER", nullable: true),
                    ProgramID = table.Column<int>(type: "INTEGER", nullable: true),
                    ProgramID1 = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chapters", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Chapters_Directors_DirectorID",
                        column: x => x.DirectorID,
                        principalTable: "Directors",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Programs",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ChapterID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programs", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Programs_Chapters_ChapterID",
                        column: x => x.ChapterID,
                        principalTable: "Chapters",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SingerPrograms",
                columns: table => new
                {
                    SingerID = table.Column<int>(type: "INTEGER", nullable: false),
                    ProgramID = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SingerPrograms", x => new { x.SingerID, x.ProgramID });
                    table.ForeignKey(
                        name: "FK_SingerPrograms_Programs_ProgramID",
                        column: x => x.ProgramID,
                        principalTable: "Programs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SingerPrograms_Singers_SingerID",
                        column: x => x.SingerID,
                        principalTable: "Singers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Volunteers_ChapterID",
                table: "Volunteers",
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
                name: "IX_Directors_FirstName_LastName_DOB",
                table: "Directors",
                columns: new[] { "FirstName", "LastName", "DOB" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chapters_DirectorID",
                table: "Chapters",
                column: "DirectorID");

            migrationBuilder.CreateIndex(
                name: "IX_Chapters_ProgramID1",
                table: "Chapters",
                column: "ProgramID1");

            migrationBuilder.CreateIndex(
                name: "IX_Programs_ChapterID",
                table: "Programs",
                column: "ChapterID");

            migrationBuilder.CreateIndex(
                name: "IX_SingerPrograms_ProgramID",
                table: "SingerPrograms",
                column: "ProgramID");

            migrationBuilder.AddForeignKey(
                name: "FK_Singers_Chapters_ChapterID",
                table: "Singers",
                column: "ChapterID",
                principalTable: "Chapters",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Volunteers_Chapters_ChapterID",
                table: "Volunteers",
                column: "ChapterID",
                principalTable: "Chapters",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chapters_Programs_ProgramID1",
                table: "Chapters",
                column: "ProgramID1",
                principalTable: "Programs",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Singers_Chapters_ChapterID",
                table: "Singers");

            migrationBuilder.DropForeignKey(
                name: "FK_Volunteers_Chapters_ChapterID",
                table: "Volunteers");

            migrationBuilder.DropForeignKey(
                name: "FK_Chapters_Programs_ProgramID1",
                table: "Chapters");

            migrationBuilder.DropTable(
                name: "SingerPrograms");

            migrationBuilder.DropTable(
                name: "Programs");

            migrationBuilder.DropTable(
                name: "Chapters");

            migrationBuilder.DropIndex(
                name: "IX_Volunteers_ChapterID",
                table: "Volunteers");

            migrationBuilder.DropIndex(
                name: "IX_Singers_ChapterID",
                table: "Singers");

            migrationBuilder.DropIndex(
                name: "IX_Singers_FirstName_LastName_DOB",
                table: "Singers");

            migrationBuilder.DropIndex(
                name: "IX_Directors_FirstName_LastName_DOB",
                table: "Directors");

            migrationBuilder.DropColumn(
                name: "ChapterID",
                table: "Volunteers");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Singers");

            migrationBuilder.DropColumn(
                name: "ChapterID",
                table: "Singers");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Singers");

            migrationBuilder.RenameColumn(
                name: "RegisterDate",
                table: "Volunteers",
                newName: "EmploymentDate");

            migrationBuilder.RenameColumn(
                name: "HireDate",
                table: "Directors",
                newName: "EmploymentDate");
        }
    }
}
