using Microsoft.EntityFrameworkCore.Migrations;

namespace MAVN.Job.QuorumTransactionWatcher.MsSqlRepositories.Migrations
{
    public partial class StoreOnlyLastIndexedBlockInDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "block_indexation_states",
                schema: "quorum_transaction_watcher");

            migrationBuilder.CreateTable(
                name: "last_indexed_block",
                schema: "quorum_transaction_watcher",
                columns: table => new
                {
                    last_indexed_block_number = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_last_indexed_block", x => x.last_indexed_block_number);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "last_indexed_block",
                schema: "quorum_transaction_watcher");

            migrationBuilder.CreateTable(
                name: "block_indexation_states",
                schema: "quorum_transaction_watcher",
                columns: table => new
                {
                    block_number = table.Column<long>(nullable: false),
                    block_hash = table.Column<string>(type: "varchar(66)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_block_indexation_states", x => x.block_number);
                });
        }
    }
}
