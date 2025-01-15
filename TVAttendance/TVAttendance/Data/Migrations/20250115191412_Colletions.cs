using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVAttendance.Data.Migrations
{
    /// <inheritdoc />
    public partial class Colletions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmergencyContacts_Singers_singerID",
                table: "EmergencyContacts");

            migrationBuilder.DropIndex(
                name: "IX_EmergencyContacts_singerID",
                table: "EmergencyContacts");

            migrationBuilder.DropColumn(
                name: "ChapterID",
                table: "Volunteers");

            migrationBuilder.DropColumn(
                name: "singerID",
                table: "EmergencyContacts");

            migrationBuilder.DropColumn(
                name: "ChapterID",
                table: "Directors");

            migrationBuilder.AddColumn<int>(
                name: "VolunteerID",
                table: "EmergencyContacts",
                type: "INTEGER",
                nullable: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_EmergencyContacts_Volunteers_VolunteerID",
                table: "EmergencyContacts",
                column: "VolunteerID",
                principalTable: "Volunteers",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmergencyContacts_Volunteers_VolunteerID",
                table: "EmergencyContacts");

            migrationBuilder.DropTable(
                name: "EmergencyContactSinger");

            migrationBuilder.DropIndex(
                name: "IX_EmergencyContacts_VolunteerID",
                table: "EmergencyContacts");

            migrationBuilder.DropColumn(
                name: "VolunteerID",
                table: "EmergencyContacts");

            migrationBuilder.AddColumn<int>(
                name: "ChapterID",
                table: "Volunteers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "singerID",
                table: "EmergencyContacts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ChapterID",
                table: "Directors",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyContacts_singerID",
                table: "EmergencyContacts",
                column: "singerID");

            migrationBuilder.AddForeignKey(
                name: "FK_EmergencyContacts_Singers_singerID",
                table: "EmergencyContacts",
                column: "singerID",
                principalTable: "Singers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
