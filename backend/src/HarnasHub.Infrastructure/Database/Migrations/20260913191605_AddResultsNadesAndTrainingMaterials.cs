using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HarnasHub.Infrastructure.Database.Migrations
{
	/// <inheritdoc />
	public partial class AddResultsNadesAndTrainingMaterials : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "MatchResults",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					Opponent = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
					OurScore = table.Column<int>(type: "integer", nullable: false),
					OpponentScore = table.Column<int>(type: "integer", nullable: false),
					MapName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
					DemoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
					Notes = table.Column<string>(type: "text", nullable: true),
					PlayedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
					CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_MatchResults", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "NadeEntries",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					MapName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
					Type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
					Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
					Description = table.Column<string>(type: "text", nullable: true),
					YoutubeUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
					CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
					CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_NadeEntries", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "TrainingMaterials",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
					Url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
					Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
					Description = table.Column<string>(type: "text", nullable: true),
					CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
					CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_TrainingMaterials", x => x.Id);
				});

			migrationBuilder.CreateIndex(
				name: "IX_MatchResults_PlayedAtUtc",
				table: "MatchResults",
				column: "PlayedAtUtc");

			migrationBuilder.CreateIndex(
				name: "IX_NadeEntries_MapName_Type",
				table: "NadeEntries",
				columns: new[] { "MapName", "Type" });
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "MatchResults");

			migrationBuilder.DropTable(
				name: "NadeEntries");

			migrationBuilder.DropTable(
				name: "TrainingMaterials");
		}
	}
}
