import { GridSettingModel } from "../shared/grid-setting-model";

export interface IApplyDepositModel {
    ad_id: number;
    ad_mm_id: number;//儲值人
    ad_amount: number;//儲值金額
    ad_key: string;//儲值後得到的KEY(要給我們才查的到)
    ad_url: string;//儲值網址(現在為流水號，利用QR CODE後需要輸入該流水號才能儲值)
    ad_file_name: string;//成功時貼上畫面所需儲存檔名
    ad_status: string;//申請狀態:0-待會員儲值、1-已匯入儲值金、2-申請駁回、3-匯入失敗
    ad_type: string;//儲值狀態:10-虛擬幣、50-銀行轉帳
    ad_creat_datetime: Date;//申請時間
    mm_account?: string;//會員帳號
}

export interface IQueryApplyDeposit extends GridSettingModel {
    mm_account?: string;//會員帳號
    mm_name?: string;//會員名稱
    ad_status?: string//申請狀態:0-待會員儲值、1-已匯入儲值金、2-申請駁回、3-匯入失敗
}

export interface IputApplyDepositModel {
    ad_id?: number;
    mm_id?: number;//覆核人員
    ad_mm_id?: number;//儲值人
    ad_status?: string;//申請狀態:0-待會員儲值、1-已匯入儲值金、2-申請駁回、3-匯入失敗
}