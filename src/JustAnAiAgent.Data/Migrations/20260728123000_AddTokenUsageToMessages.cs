using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JustAnAiAgent.Data.Migrations
{
    [Migration("20260728123000_AddTokenUsageToMessages")]
    public partial class AddTokenUsageToMessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TokenUsage",
                table: "Messages",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TokenUsage",
                table: "Messages");
        }
    }
}
