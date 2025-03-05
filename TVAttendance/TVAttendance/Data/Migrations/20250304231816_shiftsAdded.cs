using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVAttendance.Data.Migrations
{
    /// <inheritdoc />
    public partial class shiftsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VolunteerEvents");

            migrationBuilder.CreateTable(
                name: "Shifts",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EventID = table.Column<int>(type: "INTEGER", nullable: false),
                    ShiftStart = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ShiftEnd = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shifts", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Shifts_Events_EventID",
                        column: x => x.EventID,
                        principalTable: "Events",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShiftVolunteers",
                columns: table => new
                {
                    ShiftID = table.Column<int>(type: "INTEGER", nullable: false),
                    VolunteerID = table.Column<int>(type: "INTEGER", nullable: false),
                    ShiftAttended = table.Column<bool>(type: "INTEGER", nullable: false),
                    NonAttendanceNote = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftVolunteers", x => new { x.ShiftID, x.VolunteerID });
                    table.ForeignKey(
                        name: "FK_ShiftVolunteers_Shifts_ShiftID",
                        column: x => x.ShiftID,
                        principalTable: "Shifts",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShiftVolunteers_Volunteers_VolunteerID",
                        column: x => x.VolunteerID,
                        principalTable: "Volunteers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_EventID",
                table: "Shifts",
                column: "EventID");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftVolunteers_VolunteerID",
                table: "ShiftVolunteers",
                column: "VolunteerID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShiftVolunteers");

            migrationBuilder.DropTable(
                name: "Shifts");

            migrationBuilder.CreateTable(
                name: "VolunteerEvents",
                columns: table => new
                {
                    EventID = table.Column<int>(type: "INTEGER", nullable: false),
                    VolunteerID = table.Column<int>(type: "INTEGER", nullable: false),
                    NonAttendanceNote = table.Column<string>(type: "TEXT", nullable: true),
                    ShiftAttended = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShiftEnd = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ShiftStart = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VolunteerEvents", x => new { x.EventID, x.VolunteerID });
                    table.ForeignKey(
                        name: "FK_VolunteerEvents_Events_EventID",
                        column: x => x.EventID,
                        principalTable: "Events",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VolunteerEvents_Volunteers_VolunteerID",
                        column: x => x.VolunteerID,
                        principalTable: "Volunteers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerEvents_VolunteerID",
                table: "VolunteerEvents",
                column: "VolunteerID");
        }
    }
}
