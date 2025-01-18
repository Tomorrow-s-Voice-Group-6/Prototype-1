using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVAttendance.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedDataModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmergencyContactSinger");

            migrationBuilder.DropTable(
                name: "EmergencyContacts");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Singers");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Singers",
                newName: "EmergencyContactLastName");

            migrationBuilder.AddColumn<DateTime>(
                name: "DOB",
                table: "Volunteers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateOnly>(
                name: "EmploymentDate",
                table: "Volunteers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactFirstName",
                table: "Singers",
                type: "TEXT",
                maxLength: 55,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactPhone",
                table: "Singers",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DOB",
                table: "Volunteers");

            migrationBuilder.DropColumn(
                name: "EmploymentDate",
                table: "Volunteers");

            migrationBuilder.DropColumn(
                name: "EmergencyContactFirstName",
                table: "Singers");

            migrationBuilder.DropColumn(
                name: "EmergencyContactPhone",
                table: "Singers");

            migrationBuilder.RenameColumn(
                name: "EmergencyContactLastName",
                table: "Singers",
                newName: "Phone");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Singers",
                type: "TEXT",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "EmergencyContacts",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 55, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Relationship = table.Column<int>(type: "INTEGER", nullable: false),
                    VolunteerID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyContacts", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EmergencyContacts_Volunteers_VolunteerID",
                        column: x => x.VolunteerID,
                        principalTable: "Volunteers",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "EmergencyContactSinger",
                columns: table => new
                {
                    EmergencyContactsID = table.Column<int>(type: "INTEGER", nullable: false),
                    SingersID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyContactSinger", x => new { x.EmergencyContactsID, x.SingersID });
                    table.ForeignKey(
                        name: "FK_EmergencyContactSinger_EmergencyContacts_EmergencyContactsID",
                        column: x => x.EmergencyContactsID,
                        principalTable: "EmergencyContacts",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmergencyContactSinger_Singers_SingersID",
                        column: x => x.SingersID,
                        principalTable: "Singers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyContacts_VolunteerID",
                table: "EmergencyContacts",
                column: "VolunteerID");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyContactSinger_SingersID",
                table: "EmergencyContactSinger",
                column: "SingersID");
        }
    }
}
