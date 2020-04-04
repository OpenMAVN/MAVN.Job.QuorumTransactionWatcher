using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MAVN.Job.QuorumTransactionWatcher.MsSqlRepositories.Entities
{
    [Table("blocks_data")]
    public class BlocksDataEntity
    {
        [Key]
        [Column("key")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Key { get; set; }

        [Required]
        [Column("value")]
        public string Value { get; set; }
    }
}
