using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JustAnAiAgent.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageStreamStateFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsComplete",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsStillRunning",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsComplete",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsStillRunning",
                table: "Messages");
        }
    }
}
