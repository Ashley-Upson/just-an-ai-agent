using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JustAnAiAgent.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddToolCallsAndToolResponsesFieldToMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ToolCalls",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToolResponses",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ToolCalls",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ToolResponses",
                table: "Messages");
        }
    }
}