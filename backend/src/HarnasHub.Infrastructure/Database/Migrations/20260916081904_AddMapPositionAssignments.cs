using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HarnasHub.Infrastructure.Database.Migrations
{
	/// <inheritdoc />
	public partial class AddMapPositionAssignments : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "MapPositionAssignments",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					MapName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
					Side = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
					UserId = table.Column<Guid>(type: "uuid", nullable: false),
					Label = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
					X = table.Column<float>(type: "real", nullable: false),
					Y = table.Column<float>(type: "real", nullable: false),
					Note = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
					UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_MapPositionAssignments", x => x.Id);
				});

			migrationBuilder.CreateIndex(
				name: "IX_MapPositionAssignments_MapName_Side_UserId",
				table: "MapPositionAssignments",
				columns: new[] { "MapName", "Side", "UserId" },
				unique: true);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "MapPositionAssignments");
		}
	}
}
