using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabrica.Tests.Migrations
{
    /// <inheritdoc />
    public partial class AddDuNextMonthDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DueNextMonthDays",
                table: "Terms",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueNextMonthDays",
                table: "Terms");
        }
    }
}
