using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTimeEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TimeEntries");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "TimeEntries");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "TimeEntries");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "TimeEntries");

            migrationBuilder.AddColumn<DateOnly>(
                name: "Date",
                table: "TimeEntries",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "TimeEntries",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "TimeEntries");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "TimeEntries");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TimeEntries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "TimeEntries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "TimeEntries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "TimeEntries",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
