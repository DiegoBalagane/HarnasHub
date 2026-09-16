using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HarnasHub.Infrastructure.Database.Migrations
{
	/// <inheritdoc />
	public partial class AddPlayerAvailabilityAndVacations : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "PlayerAvailabilityDays",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					UserId = table.Column<Guid>(type: "uuid", nullable: false),
					Date = table.Column<DateOnly>(type: "date", nullable: false),
					Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
					AvailableFromLocal = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
					AvailableToLocal = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
					Note = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
					UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_PlayerAvailabilityDays", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Vacations",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					UserId = table.Column<Guid>(type: "uuid", nullable: false),
					StartDate = table.Column<DateOnly>(type: "date", nullable: false),
					EndDate = table.Column<DateOnly>(type: "date", nullable: false),
					Reason = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
					CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Vacations", x => x.Id);
				});

			migrationBuilder.CreateIndex(
				name: "IX_PlayerAvailabilityDays_UserId_Date",
				table: "PlayerAvailabilityDays",
				columns: new[] { "UserId", "Date" },
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_Vacations_UserId_StartDate",
				table: "Vacations",
				columns: new[] { "UserId", "StartDate" });
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "PlayerAvailabilityDays");

			migrationBuilder.DropTable(
				name: "Vacations");
		}
	}
}
