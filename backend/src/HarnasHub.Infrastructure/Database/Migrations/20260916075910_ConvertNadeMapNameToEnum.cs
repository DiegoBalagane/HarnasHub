using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HarnasHub.Infrastructure.Database.Migrations
{
	/// <inheritdoc />
	public partial class ConvertNadeMapNameToEnum : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			// Existing rows may hold maps dropped from the 2026 pool (Train, Vertigo, Overpass, free text);
			// fold them into Dust2 so every value parses into the new MapName enum.
			migrationBuilder.Sql(
				"""
				UPDATE "NadeEntries"
				SET "MapName" = 'Dust2'
				WHERE "MapName" NOT IN ('Dust2', 'Mirage', 'Inferno', 'Nuke', 'Ancient', 'Anubis');
				""");

			migrationBuilder.AlterColumn<string>(
				name: "MapName",
				table: "NadeEntries",
				type: "character varying(20)",
				maxLength: 20,
				nullable: false,
				oldClrType: typeof(string),
				oldType: "character varying(50)",
				oldMaxLength: 50);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<string>(
				name: "MapName",
				table: "NadeEntries",
				type: "character varying(50)",
				maxLength: 50,
				nullable: false,
				oldClrType: typeof(string),
				oldType: "character varying(20)",
				oldMaxLength: 20);
		}
	}
}
