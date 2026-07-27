using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JustAnAiAgent.Data.Migrations
{
    /// <inheritdoc />
    public partial class RestructureProjectConversationLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AgenticProjects_Messages_MessageId",
                table: "AgenticProjects");

            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_AgenticProjects_AgenticProjectId",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_AgenticProjectId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "AgenticProjectId",
                table: "Conversations");

            migrationBuilder.AlterColumn<Guid>(
                name: "MessageId",
                table: "AgenticProjects",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "ConversationId",
                table: "AgenticProjects",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE ap
                SET ConversationId = m.ConversationId
                FROM AgenticProjects ap
                INNER JOIN Messages m ON ap.MessageId = m.Id
                WHERE ap.ConversationId IS NULL
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "ConversationId",
                table: "AgenticProjects",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgenticProjects_ConversationId",
                table: "AgenticProjects",
                column: "ConversationId");

            migrationBuilder.AddForeignKey(
                name: "FK_AgenticProjects_Conversations_ConversationId",
                table: "AgenticProjects",
                column: "ConversationId",
                principalTable: "Conversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AgenticProjects_Messages_MessageId",
                table: "AgenticProjects",
                column: "MessageId",
                principalTable: "Messages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AgenticProjects_Conversations_ConversationId",
                table: "AgenticProjects");

            migrationBuilder.DropForeignKey(
                name: "FK_AgenticProjects_Messages_MessageId",
                table: "AgenticProjects");

            migrationBuilder.DropIndex(
                name: "IX_AgenticProjects_ConversationId",
                table: "AgenticProjects");

            migrationBuilder.DropColumn(
                name: "ConversationId",
                table: "AgenticProjects");

            migrationBuilder.AddColumn<Guid>(
                name: "AgenticProjectId",
                table: "Conversations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "MessageId",
                table: "AgenticProjects",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_AgenticProjectId",
                table: "Conversations",
                column: "AgenticProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_AgenticProjects_Messages_MessageId",
                table: "AgenticProjects",
                column: "MessageId",
                principalTable: "Messages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_AgenticProjects_AgenticProjectId",
                table: "Conversations",
                column: "AgenticProjectId",
                principalTable: "AgenticProjects",
                principalColumn: "Id");
        }
    }
}
