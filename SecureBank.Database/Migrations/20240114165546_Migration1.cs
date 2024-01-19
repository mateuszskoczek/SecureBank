using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecureBank.Database.Migrations
{
    /// <inheritdoc />
    public partial class Migration1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    LoginFailedCount = table.Column<byte>(type: "INTEGER", nullable: false),
                    TemporaryPassword = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountPasswords",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    Password = table.Column<byte[]>(type: "BLOB", maxLength: 1000, nullable: false),
                    LeftSalt = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    RightSalt = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountPasswords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountPasswords_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountPasswordIndexes",
                columns: table => new
                {
                    AccountPasswordId = table.Column<long>(type: "INTEGER", nullable: false),
                    Index = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_AccountPasswordIndexes_AccountPasswords_AccountPasswordId",
                        column: x => x.AccountPasswordId,
                        principalTable: "AccountPasswords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountPasswordIndexes_AccountPasswordId",
                table: "AccountPasswordIndexes",
                column: "AccountPasswordId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountPasswords_AccountId",
                table: "AccountPasswords",
                column: "AccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountPasswordIndexes");

            migrationBuilder.DropTable(
                name: "AccountPasswords");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}
