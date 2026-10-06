using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HarnasHub.Infrastructure.Database.Migrations
{
	/// <inheritdoc />
	public partial class AddEventGamePlan : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "EventGamePlanItems",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					EventId = table.Column<Guid>(type: "uuid", nullable: false),
					Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
					TargetId = table.Column<Guid>(type: "uuid", nullable: false),
					Order = table.Column<int>(type: "integer", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_EventGamePlanItems", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "EventGamePlans",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					EventId = table.Column<Guid>(type: "uuid", nullable: false),
					Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
					UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
					UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_EventGamePlans", x => x.Id);
				});

			migrationBuilder.CreateIndex(
				name: "IX_EventGamePlanItems_EventId_Kind_TargetId",
				table: "EventGamePlanItems",
				columns: new[] { "EventId", "Kind", "TargetId" },
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_EventGamePlans_EventId",
				table: "EventGamePlans",
				column: "EventId",
				unique: true);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "EventGamePlanItems");

			migrationBuilder.DropTable(
				name: "EventGamePlans");
		}
	}
}
