import { GridSettingModel } from "../../../@entities/shared/grid-setting-model";

export interface IQueryWalletReqModel extends GridSettingModel {
    mm_account?: string;//會員帳號
    mm_name?: string;//會員名稱
}

export interface IQueryWalletRespModel {
    mw_id?: number;//會員ID
    mm_account?: string;//會員帳號
    mm_name?: string;//會員名稱
    mw_stored?: number;//儲值點數
    mw_accumulation?:number;//得標金、累積獎勵
    mw_reward?: number;//紅利點數
    mw_mall?:number;//商城點數
}