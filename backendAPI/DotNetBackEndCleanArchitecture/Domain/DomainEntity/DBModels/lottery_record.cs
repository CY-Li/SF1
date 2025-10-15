namespace DomainEntity.DBModels;

/// [Table("lottery_record")]
public partial class lottery_record
{
    /// [Key]
    /// [Column(TypeName = "bigint(15)")]
    public long lr_id { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long lr_tm_id { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long lr_td_id { get; set; }

    /// [Column(TypeName = "int(15)")]
    public int lr_td_period { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long lr_create_member { get; set; }

    /// [Column(TypeName = "datetime")]
    public DateTime lr_create_datetime { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long lr_update_member { get; set; }

    /// [Column(TypeName = "datetime")]
    public DateTime lr_update_datetime { get; set; }
}
