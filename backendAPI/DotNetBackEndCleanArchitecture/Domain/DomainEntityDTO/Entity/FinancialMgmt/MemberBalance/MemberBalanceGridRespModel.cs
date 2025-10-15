using Swashbuckle.AspNetCore.Annotations;

namespace DomainEntityDTO.Entity.FinancialMgmt.MemberBalance
{
    /// <summary>
    /// 後台-取得帳務紀錄列表
    /// </summary>
    public class QueryMemberBalanceAdminRespModel
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        [SwaggerSchema(Description = "帳務ID")]
        public long mb_id { get; set; } = 0;

        /// [Column(TypeName = "bigint(15)")]
        [SwaggerSchema(Description = "會員ID")]
        public long mb_mm_id { get; set; } = 0;

        /// <summary>
        /// 收款人帳號(註冊點數可以轉給其他人)
        /// </summary>
        /// [Column(TypeName = "bigint(15)")]
        [SwaggerSchema(Description = "收款人")]
        public long mb_payee_mm_id { get; set; } = 0;

        /// [Column(TypeName = "bigint(15)")]
        [SwaggerSchema(Description = "標案ID")]
        public long mb_tm_id { get; set; } = 0;

        /// [Column(TypeName = "int(15)")]
        [SwaggerSchema(Description = "明細ID")]
        public int mb_td_id { get; set; } = 0;

        /// <summary>
        /// 點數流向:I-收入，O支出(寫入時全大寫，依照mm_id角度)
        /// </summary>
        /// [StringLength(1)]
        [SwaggerSchema(Description = "點數流向")]
        public string mb_income_type_name { get; set; } = string.Empty;

        /// <summary>
        /// 交易類別:11-下標扣除押金(儲值、註冊) 、12-下標扣除額(儲值、註冊) 、13-下標紅利(紅利:下層下標時給的紅利(下標當下會先查找所有上層邀請人，透過排程寫入紅利)，tr_status:10)，下標紅利入儲值點數(紅利>儲值:排程處理下層下標時給的紅利進儲值點數，tr_status:20) 、14-當期扣點(儲值點數(未死會):成組後扣除額，死會:紀錄10000(不進mb)) 、15-死會(紀錄10000) 、16-得標([紅利，包括押金:中標時新增紅利額度，tr_status:10]，[得標歸還押金(紅利>儲值:中標後，排程歸還下標時押金(這部分不進交易紀錄))、得標入點(紅利>儲值、註冊、商城:中標後，排程處理中標後入的三種點數)，tr_status:20]) 、17-贈與點數 、18-儲值點數 、19-提領點數 、20-轉換點數(儲值轉註冊)
        /// </summary>
        /// [StringLength(2)]
        [SwaggerSchema(Description = "交易類別")]
        public string mb_tr_type_name { get; set; } = string.Empty;

        /// <summary>
        /// 帳務類別:11-下標扣除押金(儲值、註冊) 、12下標扣除額(儲值、註冊) 、13-下標紅利(紅利:下層下標時給的紅利(下標當下會先查找所有上層邀請人，透過排程寫入紅利)) 、14-下標紅利入儲值點數(紅利>儲值:排程處理下層下標時給的紅利進儲值點數) 、15-當期扣點(儲值點數(未死會):成組後扣除額，死會:紀錄10000(不進mb)) 、16-得標紅利(紅利，包括押金:中標時新增紅利額度) 、17-得標歸還押金(紅利>儲值:中標後，排程歸還下標時押金(這部分不進交易紀錄)) 、18-得標入點(紅利>儲值、註冊、商城:中標後，排程處理中標後入的三種點數) 、17-贈與點數 、18-儲值點數 、19-提領點數 、20-轉換點數(儲值轉註冊)
        /// </summary>
        /// [StringLength(2)]
        [SwaggerSchema(Description = "帳務類別")]
        public string mb_type_name { get; set; } = string.Empty;

        /// <summary>
        /// 這裡會寫入點數類型代號(對應system_parameters:sp_code)
        /// </summary>
        /// [StringLength(2)]
        [SwaggerSchema(Description = "點數類型")]
        public string mb_points_type_name { get; set; } = string.Empty;

        /// <summary>
        /// 點數異動額(點數種類多所以該筆資料會寫入點數類型)
        /// </summary>
        ////// [Precision(15, 5)]
        [SwaggerSchema(Description = "點數異動額")]
        public decimal mb_change_points { get; set; } = decimal.Zero;

        /// <summary>
        /// 點數異動後額度(點數種類多所以該筆資料會寫入點數類型))
        /// </summary>
        ////// [Precision(15, 5)]
        [SwaggerSchema(Description = "點數異動後額度")]
        public decimal mb_points { get; set; } = decimal.Zero;

        /// [Column(TypeName = "bigint(15)")]
        [SwaggerSchema(Description = "新增會員")]
        public long mb_create_member { get; set; } = 0;

        /// [Column(TypeName = "datetime")]
        [SwaggerSchema(Description = "新增時間")]
        public DateTime mb_create_datetime { get; set; }

        /// [Column(TypeName = "bigint(15)")]
        //[SwaggerSchema(Description = "更新會員")]
        //public long mb_update_member { get; set; } = 0;

        /// [Column(TypeName = "datetime")]
        //[SwaggerSchema(Description = "更新時間")]
        //public DateTime mb_update_datetime { get; set; }
    }

    /// <summary>
    /// 會員-取得帳務紀錄列表
    /// </summary>
    public class QueryMemberBalanceUserRespModel
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        [SwaggerSchema(Description = "帳務ID")]
        public long mb_id { get; set; } = 0;

        /// [Column(TypeName = "bigint(15)")]
        [SwaggerSchema(Description = "會員ID")]
        public long mb_mm_id { get; set; } = 0;

        /// <summary>
        /// 收款人帳號(註冊點數可以轉給其他人)
        /// </summary>
        /// [Column(TypeName = "bigint(15)")]
        [SwaggerSchema(Description = "收款人")]
        public long mb_payee_mm_id { get; set; } = 0;

        /// [Column(TypeName = "bigint(15)")]
        [SwaggerSchema(Description = "標案ID")]
        public long mb_tm_id { get; set; } = 0;

        /// [Column(TypeName = "int(15)")]
        [SwaggerSchema(Description = "明細ID")]
        public int mb_td_id { get; set; } = 0;

        /// <summary>
        /// 點數流向:I-收入，O支出(寫入時全大寫，依照mm_id角度)
        /// </summary>
        /// [StringLength(1)]
        [SwaggerSchema(Description = "點數流向")]
        public string mb_income_type_name { get; set; } = string.Empty;

        /// <summary>
        /// 交易類別:11-下標扣除押金(儲值、註冊) 、12-下標扣除額(儲值、註冊) 、13-下標紅利(紅利:下層下標時給的紅利(下標當下會先查找所有上層邀請人，透過排程寫入紅利)，tr_status:10)，下標紅利入儲值點數(紅利>儲值:排程處理下層下標時給的紅利進儲值點數，tr_status:20) 、14-當期扣點(儲值點數(未死會):成組後扣除額，死會:紀錄10000(不進mb)) 、15-死會(紀錄10000) 、16-得標([紅利，包括押金:中標時新增紅利額度，tr_status:10]，[得標歸還押金(紅利>儲值:中標後，排程歸還下標時押金(這部分不進交易紀錄))、得標入點(紅利>儲值、註冊、商城:中標後，排程處理中標後入的三種點數)，tr_status:20]) 、17-贈與點數 、18-儲值點數 、19-提領點數 、20-轉換點數(儲值轉註冊)
        /// </summary>
        /// [StringLength(2)]
        [SwaggerSchema(Description = "交易類別")]
        public string mb_tr_type_name { get; set; } = string.Empty;

        /// <summary>
        /// 帳務類別:11-下標扣除押金(儲值、註冊) 、12下標扣除額(儲值、註冊) 、13-下標紅利(紅利:下層下標時給的紅利(下標當下會先查找所有上層邀請人，透過排程寫入紅利)) 、14-下標紅利入儲值點數(紅利>儲值:排程處理下層下標時給的紅利進儲值點數) 、15-當期扣點(儲值點數(未死會):成組後扣除額，死會:紀錄10000(不進mb)) 、16-得標紅利(紅利，包括押金:中標時新增紅利額度) 、17-得標歸還押金(紅利>儲值:中標後，排程歸還下標時押金(這部分不進交易紀錄)) 、18-得標入點(紅利>儲值、註冊、商城:中標後，排程處理中標後入的三種點數) 、17-贈與點數 、18-儲值點數 、19-提領點數 、20-轉換點數(儲值轉註冊)
        /// </summary>
        /// [StringLength(2)]
        [SwaggerSchema(Description = "帳務類別")]
        public string mb_type_name { get; set; } = string.Empty;

        /// <summary>
        /// 這裡會寫入點數類型代號(對應system_parameters:sp_code)
        /// </summary>
        /// [StringLength(2)]
        [SwaggerSchema(Description = "點數類型")]
        public string mb_points_type_name { get; set; } = string.Empty;

        /// <summary>
        /// 點數異動額(點數種類多所以該筆資料會寫入點數類型)
        /// </summary>
        ////// [Precision(15, 5)]
        [SwaggerSchema(Description = "點數異動額")]
        public decimal mb_change_points { get; set; } = decimal.Zero;

        /// <summary>
        /// 點數異動後額度(點數種類多所以該筆資料會寫入點數類型))
        /// </summary>
        ////// [Precision(15, 5)]
        [SwaggerSchema(Description = "點數異動後額度")]
        public decimal mb_points { get; set; } = decimal.Zero;

        /// [Column(TypeName = "bigint(15)")]
        [SwaggerSchema(Description = "新增會員")]
        public long mb_create_member { get; set; } = 0;

        /// [Column(TypeName = "datetime")]
        [SwaggerSchema(Description = "新增時間")]
        public DateTime mb_create_datetime { get; set; }

        /// [Column(TypeName = "bigint(15)")]
        //[SwaggerSchema(Description = "更新會員")]
        //public long mb_update_member { get; set; } = 0;

        /// [Column(TypeName = "datetime")]
        //[SwaggerSchema(Description = "更新時間")]
        //public DateTime mb_update_datetime { get; set; }
    }
}
