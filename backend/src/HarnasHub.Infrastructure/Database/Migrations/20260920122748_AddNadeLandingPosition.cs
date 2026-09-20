using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HarnasHub.Infrastructure.Database.Migrations
{
	/// <inheritdoc />
	public partial class AddNadeLandingPosition : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<float>(
				name: "LandingX",
				table: "NadeEntries",
				type: "real",
				nullable: true);

			migrationBuilder.AddColumn<float>(
				name: "LandingY",
				table: "NadeEntries",
				type: "real",
				nullable: true);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "LandingX",
				table: "NadeEntries");

			migrationBuilder.DropColumn(
				name: "LandingY",
				table: "NadeEntries");
		}
	}
}
