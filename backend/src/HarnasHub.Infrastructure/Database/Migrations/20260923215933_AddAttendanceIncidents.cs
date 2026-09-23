using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HarnasHub.Infrastructure.Database.Migrations
{
	/// <inheritdoc />
	public partial class AddAttendanceIncidents : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "AttendanceIncidents",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					UserId = table.Column<Guid>(type: "uuid", nullable: false),
					Type = table.Column<int>(type: "integer", nullable: false),
					OccurredOn = table.Column<DateOnly>(type: "date", nullable: false),
					Note = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
					RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
					CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AttendanceIncidents", x => x.Id);
				});

			migrationBuilder.CreateIndex(
				name: "IX_AttendanceIncidents_UserId_OccurredOn",
				table: "AttendanceIncidents",
				columns: new[] { "UserId", "OccurredOn" });
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "AttendanceIncidents");
		}
	}
}
