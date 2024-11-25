using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabrica.Tests.Migrations
{
    /// <inheritdoc />
    public partial class Phase2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastUpdate",
                table: "Terms",
                newName: "LastUpdatedTime");

            migrationBuilder.RenameColumn(
                name: "DueNextMonthDays",
                table: "Terms",
                newName: "Active");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateTime",
                table: "Terms",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercent",
                table: "Terms",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateTime",
                table: "Terms");

            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "Terms");

            migrationBuilder.RenameColumn(
                name: "LastUpdatedTime",
                table: "Terms",
                newName: "LastUpdate");

            migrationBuilder.RenameColumn(
                name: "Active",
                table: "Terms",
                newName: "DueNextMonthDays");
        }
    }
}
