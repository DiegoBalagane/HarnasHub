using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HarnasHub.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddHiddenOpponentsAndLifetimeMapStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MapStatsJson",
                table: "FaceitPlayers",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MapStatsSyncedAtUtc",
                table: "FaceitPlayers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HiddenOpponents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OpponentKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HiddenAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HiddenByUserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HiddenOpponents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HiddenOpponents_OpponentKey",
                table: "HiddenOpponents",
                column: "OpponentKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HiddenOpponents");

            migrationBuilder.DropColumn(
                name: "MapStatsJson",
                table: "FaceitPlayers");

            migrationBuilder.DropColumn(
                name: "MapStatsSyncedAtUtc",
                table: "FaceitPlayers");
        }
    }
}
