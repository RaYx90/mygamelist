using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameList.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsIndieToGames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsIndie",
                table: "Games",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsIndie",
                table: "Games");
        }
    }
}
