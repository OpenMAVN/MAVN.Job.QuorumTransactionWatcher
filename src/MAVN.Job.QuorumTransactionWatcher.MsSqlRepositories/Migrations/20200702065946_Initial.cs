using Microsoft.EntityFrameworkCore.Migrations;

namespace MAVN.Job.QuorumTransactionWatcher.MsSqlRepositories.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "quorum_transaction_watcher");

            migrationBuilder.CreateTable(
                name: "blocks_data",
                schema: "quorum_transaction_watcher",
                columns: table => new
                {
                    key = table.Column<string>(nullable: false),
                    value = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blocks_data", x => x.key);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blocks_data",
                schema: "quorum_transaction_watcher");
        }
    }
}
