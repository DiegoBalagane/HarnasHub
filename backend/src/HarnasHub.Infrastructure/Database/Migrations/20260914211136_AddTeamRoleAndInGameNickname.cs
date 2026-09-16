using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HarnasHub.Infrastructure.Database.Migrations
{
	/// <inheritdoc />
	public partial class AddTeamRoleAndInGameNickname : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<string>(
				name: "InGameNickname",
				table: "Users",
				type: "character varying(50)",
				maxLength: 50,
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "TeamRole",
				table: "Users",
				type: "character varying(20)",
				maxLength: 20,
				nullable: true);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "InGameNickname",
				table: "Users");

			migrationBuilder.DropColumn(
				name: "TeamRole",
				table: "Users");
		}
	}
}
