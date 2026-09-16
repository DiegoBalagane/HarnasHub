using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HarnasHub.Infrastructure.Database.Migrations
{
	/// <inheritdoc />
	public partial class AddPlayerStatsAndOpponentNotes : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "OpponentNotes",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					OpponentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
					Content = table.Column<string>(type: "text", nullable: false),
					MaterialUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
					CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
					CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_OpponentNotes", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "PlayerMatchStats",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					MatchResultId = table.Column<Guid>(type: "uuid", nullable: false),
					UserId = table.Column<Guid>(type: "uuid", nullable: false),
					Kills = table.Column<int>(type: "integer", nullable: false),
					Deaths = table.Column<int>(type: "integer", nullable: false),
					Assists = table.Column<int>(type: "integer", nullable: false),
					Adr = table.Column<double>(type: "double precision", nullable: false),
					HeadshotPercentage = table.Column<double>(type: "double precision", nullable: false),
					Rating = table.Column<double>(type: "double precision", nullable: false),
					CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_PlayerMatchStats", x => x.Id);
				});

			migrationBuilder.CreateIndex(
				name: "IX_OpponentNotes_OpponentName",
				table: "OpponentNotes",
				column: "OpponentName");

			migrationBuilder.CreateIndex(
				name: "IX_PlayerMatchStats_MatchResultId_UserId",
				table: "PlayerMatchStats",
				columns: new[] { "MatchResultId", "UserId" },
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_PlayerMatchStats_UserId",
				table: "PlayerMatchStats",
				column: "UserId");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "OpponentNotes");

			migrationBuilder.DropTable(
				name: "PlayerMatchStats");
		}
	}
}
