// ReSharper disable All
using Microsoft.EntityFrameworkCore.Migrations;

namespace MAVN.Job.QuorumTransactionWatcher.MsSqlRepositories.Migrations
{
    public partial class Initialization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "quorum_transaction_watcher");

            migrationBuilder.CreateTable(
                name: "block_indexation_states",
                schema: "quorum_transaction_watcher",
                columns: table => new
                {
                    blocknumber = table.Column<long>(name: "block_number", nullable: false),
                    blockhash = table.Column<string>(name: "block_hash", type: "varchar(66)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_block_indexation_states", x => x.blocknumber);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "block_indexation_states",
                schema: "quorum_transaction_watcher");
        }
    }
}
