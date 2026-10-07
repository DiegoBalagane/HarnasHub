using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HarnasHub.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddFaceitMatchFactionsAndAnalysisExclusions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<long>>(
                name: "ExcludedSteamIds",
                table: "MatchDemoAnalyses",
                type: "bigint[]",
                nullable: false,
                defaultValueSql: "'{}'::bigint[]");

            migrationBuilder.AddColumn<string>(
                name: "CompetitionId",
                table: "FaceitMatches",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Team1FactionId",
                table: "FaceitMatches",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Team2FactionId",
                table: "FaceitMatches",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExcludedSteamIds",
                table: "MatchDemoAnalyses");

            migrationBuilder.DropColumn(
                name: "CompetitionId",
                table: "FaceitMatches");

            migrationBuilder.DropColumn(
                name: "Team1FactionId",
                table: "FaceitMatches");

            migrationBuilder.DropColumn(
                name: "Team2FactionId",
                table: "FaceitMatches");
        }
    }
}
