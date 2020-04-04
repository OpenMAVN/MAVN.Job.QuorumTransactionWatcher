using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MAVN.Job.QuorumTransactionWatcher.MsSqlRepositories.Migrations
{
    public partial class AddIdAsPK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_last_indexed_block",
                schema: "quorum_transaction_watcher",
                table: "last_indexed_block");

            migrationBuilder.AddColumn<long>(
                name: "id",
                schema: "quorum_transaction_watcher",
                table: "last_indexed_block",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_last_indexed_block",
                schema: "quorum_transaction_watcher",
                table: "last_indexed_block",
                column: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_last_indexed_block",
                schema: "quorum_transaction_watcher",
                table: "last_indexed_block");

            migrationBuilder.DropColumn(
                name: "id",
                schema: "quorum_transaction_watcher",
                table: "last_indexed_block");

            migrationBuilder.AddPrimaryKey(
                name: "PK_last_indexed_block",
                schema: "quorum_transaction_watcher",
                table: "last_indexed_block",
                column: "last_indexed_block_number");
        }
    }
}
