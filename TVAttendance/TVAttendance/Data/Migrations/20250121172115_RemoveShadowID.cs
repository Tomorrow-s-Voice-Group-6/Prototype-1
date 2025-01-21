using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVAttendance.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveShadowID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chapters_Programs_ProgramID1",
                table: "Chapters");

            migrationBuilder.DropIndex(
                name: "IX_Chapters_ProgramID1",
                table: "Chapters");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Singers");

            migrationBuilder.DropColumn(
                name: "ProgramID",
                table: "Chapters");

            migrationBuilder.DropColumn(
                name: "ProgramID1",
                table: "Chapters");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Directors",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Directors",
                type: "TEXT",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 255);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Singers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Directors",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Directors",
                type: "TEXT",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProgramID",
                table: "Chapters",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProgramID1",
                table: "Chapters",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chapters_ProgramID1",
                table: "Chapters",
                column: "ProgramID1");

            migrationBuilder.AddForeignKey(
                name: "FK_Chapters_Programs_ProgramID1",
                table: "Chapters",
                column: "ProgramID1",
                principalTable: "Programs",
                principalColumn: "ID");
        }
    }
}
