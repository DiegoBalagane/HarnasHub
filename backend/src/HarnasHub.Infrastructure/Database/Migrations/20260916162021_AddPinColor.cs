using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HarnasHub.Infrastructure.Database.Migrations
{
	/// <inheritdoc />
	public partial class AddPinColor : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<string>(
				name: "PinColor",
				table: "Users",
				type: "character varying(20)",
				maxLength: 20,
				nullable: true);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "PinColor",
				table: "Users");
		}
	}
}
