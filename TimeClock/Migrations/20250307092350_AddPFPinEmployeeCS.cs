using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeClock.Migrations
{
    /// <inheritdoc />
    public partial class AddPFPinEmployeeCS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ProfilePicture",
                table: "Employee",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePicture",
                table: "Employee");
        }
    }
}
