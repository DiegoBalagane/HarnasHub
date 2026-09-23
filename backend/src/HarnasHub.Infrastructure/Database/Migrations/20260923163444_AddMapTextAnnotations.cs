using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HarnasHub.Infrastructure.Database.Migrations
{
	/// <inheritdoc />
	public partial class AddMapTextAnnotations : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "MapTextAnnotations",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					MapName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
					Side = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
					Text = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
					Color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
					FontSizePx = table.Column<int>(type: "integer", nullable: false),
					X = table.Column<float>(type: "real", nullable: false),
					Y = table.Column<float>(type: "real", nullable: false),
					CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
					UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_MapTextAnnotations", x => x.Id);
				});

			migrationBuilder.CreateIndex(
				name: "IX_MapTextAnnotations_MapName_Side",
				table: "MapTextAnnotations",
				columns: new[] { "MapName", "Side" });
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "MapTextAnnotations");
		}
	}
}
