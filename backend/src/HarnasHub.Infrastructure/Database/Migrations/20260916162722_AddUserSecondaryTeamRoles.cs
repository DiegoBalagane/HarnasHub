using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HarnasHub.Infrastructure.Database.Migrations
{
	/// <inheritdoc />
	public partial class AddUserSecondaryTeamRoles : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "UserSecondaryTeamRoles",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					UserId = table.Column<Guid>(type: "uuid", nullable: false),
					TeamRole = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_UserSecondaryTeamRoles", x => x.Id);
				});

			migrationBuilder.CreateIndex(
				name: "IX_UserSecondaryTeamRoles_UserId_TeamRole",
				table: "UserSecondaryTeamRoles",
				columns: new[] { "UserId", "TeamRole" },
				unique: true);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "UserSecondaryTeamRoles");
		}
	}
}
