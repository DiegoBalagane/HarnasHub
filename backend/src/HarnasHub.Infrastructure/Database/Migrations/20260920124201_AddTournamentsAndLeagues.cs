using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HarnasHub.Infrastructure.Database.Migrations
{
	/// <inheritdoc />
	public partial class AddTournamentsAndLeagues : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			// Existing rows predate this feature — default them to Scrimmage (enum value 0) rather than an
			// empty string, which HasConversion<string>() could never deserialize back into a MatchCategory.
			migrationBuilder.AddColumn<string>(
				name: "Category",
				table: "MatchResults",
				type: "character varying(20)",
				maxLength: 20,
				nullable: false,
				defaultValue: "Scrimmage");

			migrationBuilder.AddColumn<Guid>(
				name: "LeagueId",
				table: "MatchResults",
				type: "uuid",
				nullable: true);

			migrationBuilder.AddColumn<Guid>(
				name: "TournamentId",
				table: "MatchResults",
				type: "uuid",
				nullable: true);

			migrationBuilder.CreateTable(
				name: "Leagues",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
					Season = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
					Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
					CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
					CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Leagues", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Tournaments",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
					CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
					CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Tournaments", x => x.Id);
				});

			migrationBuilder.CreateIndex(
				name: "IX_MatchResults_LeagueId",
				table: "MatchResults",
				column: "LeagueId");

			migrationBuilder.CreateIndex(
				name: "IX_MatchResults_TournamentId",
				table: "MatchResults",
				column: "TournamentId");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "Leagues");

			migrationBuilder.DropTable(
				name: "Tournaments");

			migrationBuilder.DropIndex(
				name: "IX_MatchResults_LeagueId",
				table: "MatchResults");

			migrationBuilder.DropIndex(
				name: "IX_MatchResults_TournamentId",
				table: "MatchResults");

			migrationBuilder.DropColumn(
				name: "Category",
				table: "MatchResults");

			migrationBuilder.DropColumn(
				name: "LeagueId",
				table: "MatchResults");

			migrationBuilder.DropColumn(
				name: "TournamentId",
				table: "MatchResults");
		}
	}
}
