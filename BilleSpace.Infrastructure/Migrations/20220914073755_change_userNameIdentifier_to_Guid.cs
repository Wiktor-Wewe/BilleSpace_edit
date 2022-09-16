using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BilleSpace.Infrastructure.Migrations
{
    public partial class change_userNameIdentifier_to_Guid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserNameIdentifier",
                table: "Reservations",
                newName: "UserEmail");

            migrationBuilder.RenameColumn(
                name: "UserNameIdentifier",
                table: "Receptionists",
                newName: "UserEmail");

            migrationBuilder.RenameColumn(
                name: "AuthorNameIdentifier",
                table: "Offices",
                newName: "AuthorEmail");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserEmail",
                table: "Reservations",
                newName: "UserNameIdentifier");

            migrationBuilder.RenameColumn(
                name: "UserEmail",
                table: "Receptionists",
                newName: "UserNameIdentifier");

            migrationBuilder.RenameColumn(
                name: "AuthorEmail",
                table: "Offices",
                newName: "AuthorNameIdentifier");
        }
    }
}
