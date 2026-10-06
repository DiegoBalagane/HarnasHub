using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HarnasHub.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddDemoAnalysisAndOpponentReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "ThrowX",
                table: "NadeEntries",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "ThrowY",
                table: "NadeEntries",
                type: "real",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FaceitMatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FaceitMatchId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    MapNumber = table.Column<int>(type: "integer", nullable: false),
                    PlayedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MapName = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CompetitionType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CompetitionName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Team1Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Team2Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Team1Score = table.Column<int>(type: "integer", nullable: false),
                    Team2Score = table.Column<int>(type: "integer", nullable: false),
                    WinnerTeam = table.Column<int>(type: "integer", nullable: false),
                    Team1PlayerIds = table.Column<List<string>>(type: "text[]", nullable: false),
                    Team2PlayerIds = table.Column<List<string>>(type: "text[]", nullable: false),
                    FetchedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaceitMatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FaceitPlayers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Nickname = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SteamId64 = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Elo = table.Column<int>(type: "integer", nullable: true),
                    SkillLevel = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HistorySyncedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaceitPlayers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MatchDemoAnalyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchResultId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectKey = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ParserVersion = table.Column<int>(type: "integer", nullable: false),
                    RoundsCount = table.Column<int>(type: "integer", nullable: false),
                    OurTeamSteamIds = table.Column<List<long>>(type: "bigint[]", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchDemoAnalyses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OpponentDemoAnalyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OpponentKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MapName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    RawMapName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PlayedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FaceitMatchId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    FaceitMapNumber = table.Column<int>(type: "integer", nullable: true),
                    TimelineObjectKey = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ParserVersion = table.Column<int>(type: "integer", nullable: false),
                    RoundsCount = table.Column<int>(type: "integer", nullable: false),
                    TeamASteamIds = table.Column<List<long>>(type: "bigint[]", nullable: false),
                    TeamBSteamIds = table.Column<List<long>>(type: "bigint[]", nullable: false),
                    TeamANames = table.Column<List<string>>(type: "text[]", nullable: false),
                    TeamBNames = table.Column<List<string>>(type: "text[]", nullable: false),
                    OpponentSteamIds = table.Column<List<long>>(type: "bigint[]", nullable: false),
                    FactsJson = table.Column<string>(type: "text", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpponentDemoAnalyses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OpponentFaceitLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OpponentKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FaceitTeamId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PlayerIds = table.Column<List<string>>(type: "text[]", nullable: false),
                    LinkedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LinkedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSyncedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpponentFaceitLinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OpponentReportSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OpponentKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    GeneratedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataSyncedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReportJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpponentReportSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FaceitMatchPlayerStats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Nickname = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Team = table.Column<int>(type: "integer", nullable: false),
                    Kills = table.Column<int>(type: "integer", nullable: false),
                    Deaths = table.Column<int>(type: "integer", nullable: false),
                    Assists = table.Column<int>(type: "integer", nullable: false),
                    Adr = table.Column<double>(type: "double precision", nullable: true),
                    HeadshotPercent = table.Column<double>(type: "double precision", nullable: true),
                    TripleKills = table.Column<int>(type: "integer", nullable: false),
                    QuadroKills = table.Column<int>(type: "integer", nullable: false),
                    PentaKills = table.Column<int>(type: "integer", nullable: false),
                    Mvps = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaceitMatchPlayerStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaceitMatchPlayerStats_FaceitMatches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "FaceitMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FaceitMatches_FaceitMatchId_MapNumber",
                table: "FaceitMatches",
                columns: new[] { "FaceitMatchId", "MapNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FaceitMatches_PlayedAtUtc",
                table: "FaceitMatches",
                column: "PlayedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_FaceitMatchPlayerStats_MatchId_PlayerId",
                table: "FaceitMatchPlayerStats",
                columns: new[] { "MatchId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FaceitMatchPlayerStats_PlayerId",
                table: "FaceitMatchPlayerStats",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_FaceitPlayers_SteamId64",
                table: "FaceitPlayers",
                column: "SteamId64");

            migrationBuilder.CreateIndex(
                name: "IX_MatchDemoAnalyses_MatchResultId",
                table: "MatchDemoAnalyses",
                column: "MatchResultId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpponentDemoAnalyses_OpponentKey",
                table: "OpponentDemoAnalyses",
                column: "OpponentKey");

            migrationBuilder.CreateIndex(
                name: "IX_OpponentDemoAnalyses_OpponentKey_FaceitMatchId_FaceitMapNum~",
                table: "OpponentDemoAnalyses",
                columns: new[] { "OpponentKey", "FaceitMatchId", "FaceitMapNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_OpponentFaceitLinks_OpponentKey",
                table: "OpponentFaceitLinks",
                column: "OpponentKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpponentReportSnapshots_OpponentKey",
                table: "OpponentReportSnapshots",
                column: "OpponentKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FaceitMatchPlayerStats");

            migrationBuilder.DropTable(
                name: "FaceitPlayers");

            migrationBuilder.DropTable(
                name: "MatchDemoAnalyses");

            migrationBuilder.DropTable(
                name: "OpponentDemoAnalyses");

            migrationBuilder.DropTable(
                name: "OpponentFaceitLinks");

            migrationBuilder.DropTable(
                name: "OpponentReportSnapshots");

            migrationBuilder.DropTable(
                name: "FaceitMatches");

            migrationBuilder.DropColumn(
                name: "ThrowX",
                table: "NadeEntries");

            migrationBuilder.DropColumn(
                name: "ThrowY",
                table: "NadeEntries");
        }
    }
}
