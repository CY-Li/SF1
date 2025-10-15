import { GridSettingModel } from "../shared/grid-setting-model";

export class ApplyWithdraw {
    
}

export interface IApplyWithdrawModel {
    aw_id:number;
    aw_mm_id:number;//儲值人
    aw_amount: number;//儲值金額
    aw_key: string;//儲值後得到的KEY(要給我們才查的到)
    aw_url: string;//儲值網址(現在為流水號，利用QR CODE後需要輸入該流水號才能儲值)
    aw_status: string;//申請狀態:10-待管理者付款、11-已匯入會員錢包、12-申請駁回、13-匯入失敗
    aw_creat_datetime: Date;//申請時間
    mm_account?: string;//會員帳號
}

export interface IQueryApplyWithdraw extends GridSettingModel {
    mm_account?: string;//會員帳號
    mm_name?: string;//會員名稱
    aw_status?: string//申請狀態:0-待管理者儲值、1-已匯入儲值金、2-申請駁回、3-匯入失敗
}

export interface IputApplyWithdrawModel {
    aw_id?:number;
    mm_id?: number;//覆核人員
    aw_mm_id?: number;//儲值人
    aw_status?: string;//申請狀態:0-待會員儲值、1-已匯入儲值金、2-申請駁回、3-匯入失敗
}
