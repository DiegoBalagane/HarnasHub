using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HarnasHub.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerMatchStatDeathPositions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeathPositionsJson",
                table: "PlayerMatchStats",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeathPositionsJson",
                table: "PlayerMatchStats");
        }
    }
}
