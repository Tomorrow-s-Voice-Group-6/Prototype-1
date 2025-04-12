using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVAttendance.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingVolunteer2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PendingVolunteers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "PendingVolunteers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
