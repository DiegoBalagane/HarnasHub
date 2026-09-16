using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HarnasHub.Infrastructure.Database.Migrations
{
	/// <inheritdoc />
	public partial class AddEventReminderTracking : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<DateTime>(
				name: "ReminderSentAtUtc",
				table: "Events",
				type: "timestamp with time zone",
				nullable: true);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "ReminderSentAtUtc",
				table: "Events");
		}
	}
}
