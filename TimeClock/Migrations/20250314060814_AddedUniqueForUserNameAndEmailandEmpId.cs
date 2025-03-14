using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeClock.Migrations
{
    /// <inheritdoc />
    public partial class AddedUniqueForUserNameAndEmailandEmpId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Users_EmpId",
                table: "Users",
                column: "EmpId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_EmpId",
                table: "Users");
        }
    }
}
