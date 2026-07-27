using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JustAnAiAgent.Data.Migrations
{
    /// <inheritdoc />
    public partial class DropOldMessageTableColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModelResponse",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ModelThought",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "SystemPrompt",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ToolCalls",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ToolResponses",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "UserPrompt",
                table: "Messages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModelResponse",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModelThought",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SystemPrompt",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);

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

            migrationBuilder.AddColumn<string>(
                name: "UserPrompt",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
