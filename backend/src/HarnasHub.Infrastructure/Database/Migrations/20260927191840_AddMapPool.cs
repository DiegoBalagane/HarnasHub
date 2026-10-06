using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HarnasHub.Infrastructure.Database.Migrations
{
	/// <inheritdoc />
	public partial class AddMapPool : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "MapPoolEntries",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					MapName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
					Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
					Note = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
					UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
					UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_MapPoolEntries", x => x.Id);
				});

			migrationBuilder.CreateIndex(
				name: "IX_MapPoolEntries_MapName",
				table: "MapPoolEntries",
				column: "MapName",
				unique: true);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "MapPoolEntries");
		}
	}
}
