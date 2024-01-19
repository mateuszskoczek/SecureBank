using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecureBank.Database.Migrations
{
    /// <inheritdoc />
    public partial class Migration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "AccountPasswordIndexes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AccountPasswordIndexes",
                table: "AccountPasswordIndexes",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AccountPasswordIndexes",
                table: "AccountPasswordIndexes");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "AccountPasswordIndexes");
        }
    }
}
