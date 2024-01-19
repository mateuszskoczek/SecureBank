using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecureBank.Database.Migrations
{
    /// <inheritdoc />
    public partial class Migration3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountLoginRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountPasswordId = table.Column<long>(type: "INTEGER", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountLoginRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountLoginRequests_AccountPasswords_AccountPasswordId",
                        column: x => x.AccountPasswordId,
                        principalTable: "AccountPasswords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountLoginRequests_AccountPasswordId",
                table: "AccountLoginRequests",
                column: "AccountPasswordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountLoginRequests");
        }
    }
}
