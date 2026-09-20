using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HarnasHub.Infrastructure.Database.Migrations
{
	/// <inheritdoc />
	public partial class AddPlayerMatchStatDemoFields : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<int>(
				name: "EntryDeaths",
				table: "PlayerMatchStats",
				type: "integer",
				nullable: true);

			migrationBuilder.AddColumn<int>(
				name: "EntryKills",
				table: "PlayerMatchStats",
				type: "integer",
				nullable: true);

			migrationBuilder.AddColumn<int>(
				name: "FlashAssists",
				table: "PlayerMatchStats",
				type: "integer",
				nullable: true);

			migrationBuilder.AddColumn<double>(
				name: "KastPercentage",
				table: "PlayerMatchStats",
				type: "double precision",
				nullable: true);

			migrationBuilder.AddColumn<int>(
				name: "MultiKill2K",
				table: "PlayerMatchStats",
				type: "integer",
				nullable: true);

			migrationBuilder.AddColumn<int>(
				name: "MultiKill3K",
				table: "PlayerMatchStats",
				type: "integer",
				nullable: true);

			migrationBuilder.AddColumn<int>(
				name: "MultiKill4K",
				table: "PlayerMatchStats",
				type: "integer",
				nullable: true);

			migrationBuilder.AddColumn<int>(
				name: "MultiKill5K",
				table: "PlayerMatchStats",
				type: "integer",
				nullable: true);

			migrationBuilder.AddColumn<int>(
				name: "UtilityDamage",
				table: "PlayerMatchStats",
				type: "integer",
				nullable: true);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "EntryDeaths",
				table: "PlayerMatchStats");

			migrationBuilder.DropColumn(
				name: "EntryKills",
				table: "PlayerMatchStats");

			migrationBuilder.DropColumn(
				name: "FlashAssists",
				table: "PlayerMatchStats");

			migrationBuilder.DropColumn(
				name: "KastPercentage",
				table: "PlayerMatchStats");

			migrationBuilder.DropColumn(
				name: "MultiKill2K",
				table: "PlayerMatchStats");

			migrationBuilder.DropColumn(
				name: "MultiKill3K",
				table: "PlayerMatchStats");

			migrationBuilder.DropColumn(
				name: "MultiKill4K",
				table: "PlayerMatchStats");

			migrationBuilder.DropColumn(
				name: "MultiKill5K",
				table: "PlayerMatchStats");

			migrationBuilder.DropColumn(
				name: "UtilityDamage",
				table: "PlayerMatchStats");
		}
	}
}
