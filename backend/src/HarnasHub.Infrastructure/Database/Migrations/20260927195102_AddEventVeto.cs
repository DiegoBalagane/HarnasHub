using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HarnasHub.Infrastructure.Database.Migrations
{
	/// <inheritdoc />
	public partial class AddEventVeto : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "EventVetoSteps",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					EventId = table.Column<Guid>(type: "uuid", nullable: false),
					Order = table.Column<int>(type: "integer", nullable: false),
					Actor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
					Action = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
					MapName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_EventVetoSteps", x => x.Id);
				});

			migrationBuilder.CreateIndex(
				name: "IX_EventVetoSteps_EventId_MapName",
				table: "EventVetoSteps",
				columns: new[] { "EventId", "MapName" },
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_EventVetoSteps_EventId_Order",
				table: "EventVetoSteps",
				columns: new[] { "EventId", "Order" },
				unique: true);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "EventVetoSteps");
		}
	}
}
