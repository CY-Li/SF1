using DomainEntity.DBModels;
using DomainEntity.Shared;
using InfraCommon.DBA;
using System.Collections.Concurrent;

namespace InfraCommon.SharedMethod
{
    public class GetLevleData
    {
        private readonly IDBAccess _dbAccess;
        public GetLevleData(IDBAccess dbAccess)
        {
            _dbAccess = dbAccess;
        }

        public int GetLevel(long mm_invite_code)
        {
            int mResult = 0;

            try
            {
                string mSqlGetLevelData = @"
WITH RECURSIVE GetMemberLevel AS
(
       # 初始查詢：下標需查詢是否有上層會員，再依上層會會員等級給予紅利
       # 所以下面條件mm_account需要以邀請碼來做尋找
       SELECT     A.mm_id,
                  A.mm_account,
                  A.mm_invite_code,
                  B.mw_subscripts_count,
                  1                         AS class_level,
                  CAST(NULL AS VARCHAR(20)) AS class2_mm_account,
                  CAST(NULL AS VARCHAR(20)) AS class3_mm_account,
                  CAST(NULL AS VARCHAR(20)) AS class4_mm_account,
                  CAST(NULL AS VARCHAR(20)) AS class5_mm_account,
                  CAST(NULL AS VARCHAR(20)) AS class6_mm_account
       FROM       rosca2.member_master      AS A
       INNER JOIN rosca2.member_wallet AS B
               ON B.mw_mm_id = A.mm_id 
       #WHERE  A.mm_account = @mm_account
       WHERE      A.mm_id = @mm_id
       
       UNION ALL
       
       # 遞迴查詢：選取該會員的[所有從屬會員]和[從屬會員的從屬會員]
       SELECT     C.mm_id,
                  C.mm_account,
                  C.mm_invite_code,
                  D.mw_subscripts_count,
                  E.class_level + 1,
                  CASE WHEN E.class_level = 1 THEN C.mm_account ELSE E.class2_mm_account END AS class2_mm_account,
                  CASE WHEN E.class_level = 2 THEN C.mm_account ELSE E.class3_mm_account END AS class3_mm_account,
                  CASE WHEN E.class_level = 3 THEN C.mm_account ELSE E.class4_mm_account END AS class4_mm_account,
                  CASE WHEN E.class_level = 4 THEN C.mm_account ELSE E.class5_mm_account END AS class5_mm_account,
                  CASE WHEN E.class_level = 5 THEN C.mm_account ELSE E.class6_mm_account END AS class6_mm_account
       FROM       rosca2.member_master AS C
       INNER JOIN rosca2.member_wallet AS D
               ON D.mw_mm_id = C.mm_id 
       INNER JOIN GetMemberLevel       AS E
       #ON         B.mm_invite_code = C.mm_account 
       ON         E.mm_id = C.mm_invite_code
)

# 最終查詢：選取遞迴中所有會員的 mm_account, mm_invite_code, mw_subscripts_count, class(該欄位為該會原是屬於第幾階層從屬會員), class2_mm_account
SELECT mm_id,
       mm_account,
       mm_invite_code,
       mw_subscripts_count,
       class_level,
       SUM(mw_subscripts_count) OVER() AS total_qty,
       class2_mm_account,
       SUM(mw_subscripts_count) OVER(PARTITION BY class2_mm_account) AS class2_qty,
       class3_mm_account,
       SUM(mw_subscripts_count) OVER(PARTITION BY class3_mm_account) AS class3_qty,
       class4_mm_account,
       SUM(mw_subscripts_count) OVER(PARTITION BY class4_mm_account) AS class4_qty,
       class5_mm_account,
       SUM(mw_subscripts_count) OVER(PARTITION BY class5_mm_account) AS class5_qty,
       class6_mm_account,
       SUM(mw_subscripts_count) OVER(PARTITION BY class6_mm_account) AS class6_qty
FROM   GetMemberLevel
ORDER  BY class_level,
          mm_account,
          mm_invite_code 
";
                #region 轉成資料使用mm_id當檢查
                //WITH RECURSIVE GetMemberLevel AS
                //(
                //# 初始查詢：下標需查詢是否有上層會員，再依上層會會員等級給予紅利
                //# 所以下面條件mm_account需要以邀請碼來做尋找
                //       SELECT A.mm_id,
                //                  A.mm_account,
                //                  A.mm_invite_code,
                //                  B.mw_subscripts_count,
                //                  1                         AS class_level,
                //                  0 AS class2_mm_account,
                //                  0 AS class3_mm_account,
                //                  0 AS class4_mm_account,
                //                  0 AS class5_mm_account,
                //                  0 AS class6_mm_account
                //       FROM rosca2.member_master AS A
                //       INNER JOIN rosca2.member_wallet AS B
                //               ON B.mw_mm_id = A.mm_id
                //       #WHERE  A.mm_account = @mm_account
                //       WHERE A.mm_id = 1


                //       UNION ALL

                //       # 遞迴查詢：選取該會員的[所有從屬會員]和[從屬會員的從屬會員]
                //       SELECT C.mm_id,
                //                  C.mm_account,
                //                  C.mm_invite_code,
                //                  D.mw_subscripts_count,
                //                  E.class_level + 1,
                //                  CASE WHEN E.class_level = 1 THEN C.mm_id ELSE E.class2_mm_account END AS class2_mm_account,
                //                  CASE WHEN E.class_level = 2 THEN C.mm_id ELSE E.class3_mm_account END AS class3_mm_account,
                //                  CASE WHEN E.class_level = 3 THEN C.mm_id ELSE E.class4_mm_account END AS class4_mm_account,
                //                  CASE WHEN E.class_level = 4 THEN C.mm_id ELSE E.class5_mm_account END AS class5_mm_account,
                //                  CASE WHEN E.class_level = 5 THEN C.mm_id ELSE E.class6_mm_account END AS class6_mm_account
                //       FROM rosca2.member_master AS C
                //       INNER JOIN rosca2.member_wallet AS D
                //               ON D.mw_mm_id = C.mm_id
                //       INNER JOIN GetMemberLevel AS E
                //# ON         B.mm_invite_code = C.mm_account 
                //       ON         E.mm_id = C.mm_invite_code
                //)

                //# 最終查詢：選取遞迴中所有會員的 mm_account, mm_invite_code, mw_subscripts_count, class(該欄位為該會原是屬於第幾階層從屬會員), class2_mm_account
                //SELECT mm_id,
                //       mm_account,
                //       mm_invite_code,
                //       mw_subscripts_count,
                //       class_level,
                //       SUM(mw_subscripts_count) OVER() AS total_qty,
                //                class2_mm_account,
                //       SUM(mw_subscripts_count) OVER(PARTITION BY class2_mm_account) AS class2_qty,
                //                class3_mm_account,
                //       SUM(mw_subscripts_count) OVER(PARTITION BY class3_mm_account) AS class3_qty,
                //                class4_mm_account,
                //       SUM(mw_subscripts_count) OVER(PARTITION BY class4_mm_account) AS class4_qty,
                //       class5_mm_account,
                //       SUM(mw_subscripts_count) OVER(PARTITION BY class5_mm_account) AS class5_qty,
                //       class6_mm_account,
                //       SUM(mw_subscripts_count) OVER(PARTITION BY class6_mm_account) AS class6_qty
                //FROM GetMemberLevel
                //ORDER BY class_level,
                //          mm_account,
                //          mm_invite_code
                #endregion

                List<GetLevleDataDTO> mGetLevelData = _dbAccess.GetObject<GetLevleDataDTO>(mSqlGetLevelData, new { mm_id = mm_invite_code }).ToList();

                mResult = CheckLevelProcess(mGetLevelData);
            }
            catch (Exception)
            {
                throw;
            }
            return mResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inviteData">邀請人下線所有下標數量的資料</param>
        public int CheckLevelProcess(List<GetLevleDataDTO> inviteData)
        {
            int mUpperLevel = 0;//如有上層，儲存上層等級
            int mTotalQty = 0;//用來存放計算等級的會員包括下線的所有下標數，總數量達到某個一個條件才會開始執行等級檢查
            int mClassLevel = 0;//用來存放階層數量的，因條件每個等級一定會符合某個階層數量
            bool mLevelFlage = false;//判斷是否計算出等級的指標

            //用來判斷是否符合等級條件，因為是要在每個階層篩選出來的上層往下才算符合資格
            List<GetLevleDataDTO>? mKeepClassData;//每要篩選一個等級就會去掉不符合條件的，所以會每一層減少一部分資料
            List<GetLevleDataDTO>? mCheckClassData;//每要進入一層IF，就會先刷新該層所需資料
                                                   //List<string>? mCheckLevelMmid;//每進入一層IF刷新

            //不為null並且筆數大0才開始檢查
            if (inviteData != null && inviteData.Count > 0)
            {
                //取得上層會員總下標數量
                //int m_totalQty = m_getLevelData.Find(f => f.class_level == 1).total_qty;
                GetLevleDataDTO? mClass1Data = inviteData.Find(f => f.class_level == 1);

                if (mClass1Data != null)
                {
                    mTotalQty = mClass1Data.total_qty - mClass1Data.mw_subscripts_count;
                    mClassLevel = inviteData.Max(m => m.class_level);
                }

                #region L8檢查
                if (mTotalQty >= 25920
                    && mClassLevel > 5
                    && !mLevelFlage)
                {
                    //條件下線有3個L7
                    //開始判斷第2階層從屬會員，至少3個下標總數超過 8640 進入IF
                    mKeepClassData = inviteData.Where(w => w.class_level >= 2).ToList();
                    mCheckClassData = mKeepClassData.Where(w => w.class_level == 2 && (w.class2_qty - w.mw_subscripts_count) >= 8640).ToList();
                    //是否有3個L6判斷
                    if (mCheckClassData.Count() >= 3)
                    {
                        //篩選判斷L6所需資料，因需要符合L7的下線有符合L6的資料所以使用mCheckClassData.Select來篩選資料
                        mKeepClassData = mKeepClassData.Where(w => w.class_level >= 3
                                                                && mCheckClassData.Select(s => s.mm_account).Contains(w.class2_mm_account)).ToList();
                        //篩選符合L6資料的條件，至少3個下標總數超過 2880 進入IF
                        mCheckClassData = mKeepClassData.Where(w => w.class_level == 3 && (w.class3_qty - w.mw_subscripts_count) >= 2880).ToList();

                        //是否有3個L6判斷
                        if (mCheckClassData.Count() >= 3)
                        {
                            //篩選判斷L5所需資料，因需要符合L7的下線有符合L6的資料所以使用mCheckClassData.Select來篩選資料
                            mKeepClassData = mKeepClassData.Where(w => w.class_level >= 4
                                                                    && mCheckClassData.Select(s => s.mm_account).Contains(w.class3_mm_account)).ToList();
                            //篩選符合L5資料的條件，至少3個下標總數超過 960 進入IF
                            mCheckClassData = mKeepClassData.Where(w => w.class_level == 4 && (w.class4_qty - w.mw_subscripts_count) >= 960).ToList();

                            //是否有3個L5判斷
                            if (mCheckClassData.Count() >= 3)
                            {
                                //篩選判斷L4所需資料，因需要符合L7的下線有符合L6的資料所以使用mCheckClassData.Select來篩選資料
                                mKeepClassData = mKeepClassData.Where(w => w.class_level >= 5
                                                                        && mCheckClassData.Select(s => s.mm_account).Contains(w.class4_mm_account)).ToList();
                                //篩選符合L4資料的條件，至少3個下標總數超過 320 進入IF
                                mCheckClassData = mKeepClassData.Where(w => w.class_level == 5 && (w.class5_qty - w.mw_subscripts_count) >= 320).ToList();

                                //是否有3個L4判斷
                                if (mCheckClassData.Count() >= 3)
                                {
                                    mKeepClassData = mKeepClassData.Where(w => w.class_level == 6 && mCheckClassData.Select(s => s.mm_account).Contains(w.class5_mm_account)).ToList();
                                    mLevelFlage = mKeepClassData.GroupBy(g => g.class5_mm_account)
                                                                .Select(s => s.OrderByDescending(d => d.class6_qty)
                                                                              .Skip(1)
                                                                              .Sum(ss => ss.class6_qty))
                                                                .Count(total => total >= 160) >= 3;

                                    //mLevelFlage = mKeepClassData.GroupBy(w => w.class5_mm_account)
                                    //                            .Select(s => s.OrderByDescending(d => d.mw_subscripts_count)
                                    //                                           .Skip(1)
                                    //                                           .Sum(x => x.mw_subscripts_count))
                                    //                            .Where(sum => sum > 160).Count() >= 3;


                                    //是否有3個L4判斷，L4要判斷兩次的原因是因為需要檢查自己與下一階層
                                    if (mLevelFlage)
                                    {
                                        mUpperLevel = 8;
                                        mLevelFlage = true;
                                    }
                                }
                            }
                        }
                    }
                }

                mKeepClassData = null;//每檢查下一個等級前重置
                mCheckClassData = null;//每檢查下一個等級前重置
                #endregion L8檢查

                #region L7檢查
                if (mTotalQty >= 8640
                    && mClassLevel > 4
                    && !mLevelFlage)
                {
                    //條件下線有3個L6
                    //開始判斷第2階層從屬會員，至少3個下標總數超過 2880 進入IF
                    mKeepClassData = inviteData.Where(w => w.class_level >= 2).ToList();
                    mCheckClassData = mKeepClassData.Where(w => w.class_level == 2 && (w.class2_qty - w.mw_subscripts_count) >= 2880).ToList();

                    //是否有3個L6判斷
                    if (mCheckClassData.Count() >= 3)
                    {
                        //篩選判斷L5所需資料，因需要符合L6的下線有符合L6的資料所以使用mCheckClassData.Select來篩選資料
                        mKeepClassData = mKeepClassData.Where(w => w.class_level >= 3
                                                                && mCheckClassData.Select(s => s.mm_account).Contains(w.class2_mm_account)).ToList();
                        //篩選符合L5資料的條件，至少3個下標總數超過 960 進入IF
                        mCheckClassData = mKeepClassData.Where(w => w.class_level == 3 && (w.class3_qty - w.mw_subscripts_count) >= 960).ToList();

                        //是否有3個L5判斷
                        if (mCheckClassData.Count() >= 3)
                        {
                            //篩選判斷L4所需資料，因需要符合L7的下線有符合L6的資料所以使用mCheckClassData.Select來篩選資料
                            mKeepClassData = mKeepClassData.Where(w => w.class_level >= 4
                                                                    && mCheckClassData.Select(s => s.mm_account).Contains(w.class3_mm_account)).ToList();
                            //篩選符合L4資料的條件，至少3個下標總數超過 320 進入IF
                            mCheckClassData = mKeepClassData.Where(w => w.class_level == 4 && (w.class4_qty - w.mw_subscripts_count) >= 320).ToList();

                            //是否有3個L4判斷
                            if (mCheckClassData.Count() >= 3)
                            {
                                mKeepClassData = mKeepClassData.Where(w => w.class_level == 5 && mCheckClassData.Select(s => s.mm_account).Contains(w.class4_mm_account)).ToList();
                                mLevelFlage = mKeepClassData.GroupBy(g => g.class4_mm_account)
                                                            .Select(s => s.OrderByDescending(d => d.class5_qty)
                                                                          .Skip(1)
                                                                          .Sum(ss => ss.class5_qty))
                                                            .Count(total => total >= 160) >= 3;

                                //mKeepClassData = mKeepClassData.Where(w => mCheckClassData.Select(s => s.mm_account).Contains(w.class4_mm_account)).ToList();
                                //mLevelFlage = mKeepClassData.GroupBy(w => w.class4_mm_account)
                                //                            .Select(s => s.OrderByDescending(d => d.mw_subscripts_count)
                                //                                           .Skip(1)
                                //                                           .Sum(x => x.mw_subscripts_count))
                                //                            .Where(sum => sum > 160).Count() >= 3;

                                //是否有3個L4判斷，L4要判斷兩次的原因是因為需要檢查自己與下一階層
                                if (mLevelFlage)
                                {
                                    mUpperLevel = 7;
                                    mLevelFlage = true;
                                }
                            }
                        }
                    }
                }

                mKeepClassData = null;//每檢查下一個等級前重置
                mCheckClassData = null;//每檢查下一個等級前重置
                #endregion L7檢查

                #region L6檢查
                if (mTotalQty >= 2880
                    && mClassLevel > 3
                    && !mLevelFlage)
                {
                    //條件下線有3個L5
                    //開始判斷第2階層從屬會員，至少3個下標總數超過 960 進入IF
                    mKeepClassData = inviteData.Where(w => w.class_level >= 2).ToList();
                    mCheckClassData = mKeepClassData.Where(w => w.class_level == 2 && (w.class2_qty - w.mw_subscripts_count) >= 960).ToList();

                    //是否有3個L5判斷
                    if (mCheckClassData.Count() >= 3)
                    {
                        //篩選判斷L4所需資料，因需要符合L5的下線有符合L4的資料所以使用mCheckClassData.Select來篩選資料
                        mKeepClassData = mKeepClassData.Where(w => w.class_level >= 3
                                                                && mCheckClassData.Select(s => s.mm_account).Contains(w.class2_mm_account)).ToList();
                        //篩選符合L4資料的條件，至少3個下標總數超過 320 進入IF
                        mCheckClassData = mKeepClassData.Where(w => w.class_level == 3 && (w.class3_qty - w.mw_subscripts_count) >= 320).ToList();

                        //是否有3個L4判斷
                        if (mCheckClassData.Count() >= 3)
                        {
                            mKeepClassData = mKeepClassData.Where(w => w.class_level == 4 && mCheckClassData.Select(s => s.mm_account).Contains(w.class3_mm_account)).ToList();
                            mLevelFlage = mKeepClassData.GroupBy(g => g.class3_mm_account)
                                                        .Select(s => s.OrderByDescending(d => d.class4_qty)
                                                                      .Skip(1)
                                                                      .Sum(ss => ss.class4_qty))
                                                        .Count(total => total >= 160) >= 3;

                            //mKeepClassData = mKeepClassData.Where(w => mCheckClassData.Select(s => s.mm_account).Contains(w.class3_mm_account)).ToList();
                            //mLevelFlage = mKeepClassData.GroupBy(w => w.class3_mm_account)
                            //                            .Select(s => s.OrderByDescending(d => d.mw_subscripts_count)
                            //                                           .Skip(1)
                            //                                           .Sum(x => x.mw_subscripts_count))
                            //                            .Where(sum => sum > 160).Count() >= 3;

                            //是否有3個L4判斷，L4要判斷兩次的原因是因為需要檢查自己與下一階層
                            if (mLevelFlage)
                            {
                                mUpperLevel = 6;
                                mLevelFlage = true;
                            }
                        }
                    }
                }

                mKeepClassData = null;//每檢查下一個等級前重置
                mCheckClassData = null;//每檢查下一個等級前重置
                #endregion L6檢查

                #region L5檢查
                if (mTotalQty >= 960
                    && mClassLevel > 2
                    && !mLevelFlage)
                {
                    //條件下線有3個L4
                    //開始判斷第2階層從屬會員，至少3個下標總數超過 320 進入IF
                    mKeepClassData = inviteData.Where(w => w.class_level >= 2).ToList();
                    mCheckClassData = mKeepClassData.Where(w => w.class_level == 2 && (w.class2_qty - w.mw_subscripts_count) >= 320).ToList();

                    //是否有3個L4判斷
                    if (mCheckClassData.Count() >= 3)
                    {
                        mKeepClassData = mKeepClassData.Where(w => w.class_level == 3 && mCheckClassData.Select(s => s.mm_account).Contains(w.class2_mm_account)).ToList();
                        mLevelFlage = mKeepClassData.GroupBy(g => g.class2_mm_account)
                                                    .Select(s => s.OrderByDescending(d => d.class3_qty)
                                                                  .Skip(1)
                                                                  .Sum(ss => ss.class3_qty))
                                                    .Count(total => total >= 160) >= 3;

                        //mKeepClassData = mKeepClassData.Where(w => mCheckClassData.Select(s => s.mm_account).Contains(w.class2_mm_account)).ToList();
                        //mLevelFlage = mKeepClassData.GroupBy(w => w.class2_mm_account)
                        //                            .Select(s => s.OrderByDescending(d => d.mw_subscripts_count)
                        //                                           .Skip(1)
                        //                                           .Sum(x => x.mw_subscripts_count))
                        //                            .Where(sum => sum > 160).Count() >= 3;

                        //是否有3個L4判斷，L4要判斷兩次的原因是因為需要檢查自己與下一階層
                        if (mLevelFlage)
                        {
                            mUpperLevel = 5;
                            mLevelFlage = true;
                        }
                    }
                }

                mKeepClassData = null;//每檢查下一個等級前重置
                mCheckClassData = null;//每檢查下一個等級前重置
                #endregion L5檢查

                #region L4檢查
                if (mTotalQty >= 320
                    && mClassLevel > 1
                    && !mLevelFlage)
                {
                    mLevelFlage = inviteData.Where(w => w.class_level == 2).Select(s => s.class2_qty)
                                            .OrderByDescending(o => o)
                                            .Skip(1)
                                            .Sum() >= 160;
                    if (mLevelFlage)
                    {
                        mUpperLevel = 4;
                        mLevelFlage = true;
                    }
                }
                #endregion L4檢查

                #region L3檢查
                if (mTotalQty >= 160 && !mLevelFlage)
                {
                    mLevelFlage = inviteData.Where(w => w.class_level == 2).Select(s => s.class2_qty)
                                            .OrderByDescending(o => o)
                                            .Skip(1)
                                            .Sum() >= 80;
                    if (mLevelFlage)
                    {
                        mUpperLevel = 3;
                        mLevelFlage = true;
                    }
                }
                #endregion L3檢查

                #region L2檢查
                if (mTotalQty >= 80 && !mLevelFlage)
                {
                    mLevelFlage = inviteData.Where(w => w.class_level == 2).Select(s => s.class2_qty)
                                            .OrderByDescending(o => o)
                                            .Skip(1)
                                            .Sum() >= 40;
                    if (mLevelFlage)
                    {
                        mUpperLevel = 2;
                        mLevelFlage = true;
                    }
                }
                #endregion L2檢查

                #region L1檢查
                if (mTotalQty >= 40 && !mLevelFlage)
                {
                    mUpperLevel = 1;
                    mLevelFlage = true;
                }
                #endregion L1檢查
            }

            return mUpperLevel;
        }
    }
}


#region SQL
//TSQL語法
//            string m_sqlGetLevelData = @"
//WITH GetMemberLevel AS
//(
//       -- 初始查詢：投標需查詢是否有上層會員，再依上層會會員等級給予紅利
//       -- 所以下面條件mm_id需要以邀請碼來做尋找
//       SELECT A.mm_id,
//              A.mm_invite_code,
//              A.mw_subscripts_count,
//              1                          AS class_level,
//              Cast(NULL AS NVARCHAR(50)) AS class2_mm_id,
//			  CAST(NULL AS NVARCHAR(50)) AS class3_mm_id,
//			  CAST(NULL AS NVARCHAR(50)) AS class4_mm_id,
//			  CAST(NULL AS NVARCHAR(50)) AS class5_mm_id,
//			  CAST(NULL AS NVARCHAR(50)) AS class6_mm_id
//       FROM   member_master              AS A
//       WHERE  A.mm_id = @mm_id

//       UNION ALL

//       -- 遞迴查詢：選取該會員的[所有從屬會員]和[從屬會員的從屬會員]
//       SELECT     B.mm_id,
//                  B.mm_invite_code,
//                  B.mw_subscripts_count,
//                  C.class_level + 1,
//                  CASE
//                             WHEN C.class_level = 1 THEN B.mm_id
//                             ELSE C.class2_mm_id
//                  END            AS class2_mm_id,
//				  CASE WHEN C.class_level = 2 THEN B.mm_id ELSE C.class3_mm_id END AS class3_mm_id,
//				  CASE WHEN C.class_level = 3 THEN B.mm_id ELSE C.class4_mm_id END AS class4_mm_id,
//				  CASE WHEN C.class_level = 4 THEN B.mm_id ELSE C.class5_mm_id END AS class5_mm_id,
//				  CASE WHEN C.class_level = 5 THEN B.mm_id ELSE C.class6_mm_id END AS class6_mm_id
//       FROM       member_master  AS B
//       INNER JOIN GetMemberLevel AS C
//       ON         B.mm_invite_code = C.mm_id 
//)
#endregion

