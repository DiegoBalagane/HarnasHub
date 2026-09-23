using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HarnasHub.Infrastructure.Database.Migrations
{
	/// <inheritdoc />
	public partial class TaskStatusReviewFlow_MaterialCategoryEnum : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			// Normalize pre-existing free-text categories into the new MaterialCategory enum names
			// before narrowing the column, so no row is left holding a value the enum converter can't parse.
			migrationBuilder.Sql(@"
                UPDATE ""TrainingMaterials""
                SET ""Category"" = CASE
                    WHEN ""Category"" IS NULL THEN NULL
                    WHEN ""Category"" ILIKE '%granat%' OR ""Category"" ILIKE '%nade%' OR ""Category"" ILIKE '%grenade%' THEN 'Grenades'
                    WHEN ""Category"" ILIKE '%taktyk%' OR ""Category"" ILIKE '%tactic%' OR ""Category"" ILIKE '%strat%' THEN 'Tactics'
                    WHEN ""Category"" ILIKE '%strzel%' OR ""Category"" ILIKE '%cel%' OR ""Category"" ILIKE '%aim%' THEN 'Aim'
                    WHEN ""Category"" ILIKE '%pozycj%' OR ""Category"" ILIKE '%position%' THEN 'Positioning'
                    WHEN ""Category"" ILIKE '%vod%' OR ""Category"" ILIKE '%demo%' OR ""Category"" ILIKE '%analiz%' OR ""Category"" ILIKE '%review%' THEN 'VodReview'
                    WHEN ""Category"" ILIKE '%komunikac%' OR ""Category"" ILIKE '%call%' OR ""Category"" ILIKE '%communic%' THEN 'Communication'
                    ELSE 'Other'
                END;
            ");

			migrationBuilder.AlterColumn<string>(
				name: "Category",
				table: "TrainingMaterials",
				type: "character varying(20)",
				maxLength: 20,
				nullable: true,
				oldClrType: typeof(string),
				oldType: "character varying(50)",
				oldMaxLength: 50,
				oldNullable: true);

			migrationBuilder.AlterColumn<string>(
				name: "Status",
				table: "Tasks",
				type: "character varying(20)",
				maxLength: 20,
				nullable: false,
				oldClrType: typeof(string),
				oldType: "character varying(10)",
				oldMaxLength: 10);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<string>(
				name: "Category",
				table: "TrainingMaterials",
				type: "character varying(50)",
				maxLength: 50,
				nullable: true,
				oldClrType: typeof(string),
				oldType: "character varying(20)",
				oldMaxLength: 20,
				oldNullable: true);

			migrationBuilder.AlterColumn<string>(
				name: "Status",
				table: "Tasks",
				type: "character varying(10)",
				maxLength: 10,
				nullable: false,
				oldClrType: typeof(string),
				oldType: "character varying(20)",
				oldMaxLength: 20);
		}
	}
}
