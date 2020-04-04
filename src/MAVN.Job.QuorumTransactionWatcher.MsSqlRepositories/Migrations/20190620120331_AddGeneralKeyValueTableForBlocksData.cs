using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MAVN.Job.QuorumTransactionWatcher.MsSqlRepositories.Migrations
{
    public partial class AddGeneralKeyValueTableForBlocksData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "last_indexed_block",
                schema: "quorum_transaction_watcher");

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

            migrationBuilder.CreateTable(
                name: "last_indexed_block",
                schema: "quorum_transaction_watcher",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    last_indexed_block_number = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_last_indexed_block", x => x.id);
                });
        }
    }
}
