using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HarnasHub.Infrastructure.Database.Migrations
{
	/// <inheritdoc />
	public partial class SplitAccessLevelAndCoach : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.RenameColumn(
				name: "Role",
				table: "Users",
				newName: "AccessLevel");

			migrationBuilder.AddColumn<bool>(
				name: "IsCoach",
				table: "Users",
				type: "boolean",
				nullable: false,
				defaultValue: false);

			// The access level is persisted as the enum name (character varying(20)), not as an int, so dropping the old
			// "Coach" member is a pure string rewrite: former coaches keep team-member permissions as Player and carry
			// their coaching duty in the new IsCoach flag instead. "Guest"/"Player"/"Manager" rows are already valid.
			// Runs after the rename and the AddColumn above, so both target columns exist by now.
			migrationBuilder.Sql("""
                UPDATE "Users"
                SET "IsCoach" = TRUE,
                    "AccessLevel" = 'Player'
                WHERE "AccessLevel" = 'Coach';
                """);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			// Fold the coach flag back into the old single-field model before the column disappears. Managers are left
			// alone on purpose: the old model had no way to be both, and rewriting them to 'Coach' would silently strip
			// a manager's permissions on rollback, which is worse than losing the coaching tag.
			migrationBuilder.Sql("""
                UPDATE "Users"
                SET "AccessLevel" = 'Coach'
                WHERE "IsCoach" = TRUE
                  AND "AccessLevel" <> 'Manager';
                """);

			migrationBuilder.DropColumn(
				name: "IsCoach",
				table: "Users");

			migrationBuilder.RenameColumn(
				name: "AccessLevel",
				table: "Users",
				newName: "Role");
		}
	}
}
