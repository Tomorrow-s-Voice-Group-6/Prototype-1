using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVAttendance.Data.Migrations
{
    /// <inheritdoc />
    public partial class Events : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EventName = table.Column<string>(type: "TEXT", nullable: false),
                    EventStreet = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    EventCity = table.Column<string>(type: "TEXT", maxLength: 35, nullable: false),
                    EventPostalCode = table.Column<string>(type: "TEXT", maxLength: 6, nullable: false),
                    EventProvince = table.Column<int>(type: "INTEGER", nullable: false),
                    EventStart = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EventEnd = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "VolunteerEvents",
                columns: table => new
                {
                    EventID = table.Column<int>(type: "INTEGER", nullable: false),
                    VolunteerID = table.Column<int>(type: "INTEGER", nullable: false),
                    ShiftAttended = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShiftStart = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ShiftEnd = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NonAttendanceNote = table.Column<string>(type: "TEXT", nullable: true)
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
                name: "IX_Events_EventName_EventStreet",
                table: "Events",
                columns: new[] { "EventName", "EventStreet" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerEvents_VolunteerID",
                table: "VolunteerEvents",
                column: "VolunteerID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VolunteerEvents");

            migrationBuilder.DropTable(
                name: "Events");
        }
    }
}
