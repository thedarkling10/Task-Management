using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagementApp.Migrations
{
    /// <inheritdoc />
    public partial class MigrationBeta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Date",
                table: "ProjectSummaries",
                newName: "GeneratedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GeneratedAt",
                table: "ProjectSummaries",
                newName: "Date");
        }
    }
}
