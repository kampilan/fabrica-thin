using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabrica.Tests.Migrations
{
    /// <inheritdoc />
    public partial class AddTimestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LocalTimestamp",
                table: "Terms",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "RemoteTimestamp",
                table: "Terms",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocalTimestamp",
                table: "Terms");

            migrationBuilder.DropColumn(
                name: "RemoteTimestamp",
                table: "Terms");
        }
    }
}
