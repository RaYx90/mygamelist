using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameList.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSummaryEs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SummaryEs",
                table: "Games",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SummaryEs",
                table: "Games");
        }
    }
}
