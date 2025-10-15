import { GridSettingModel } from "../shared/grid-setting-model";

export interface IQueryKycReq extends GridSettingModel {
    akc_id?: number;//代號
    mm_account?: string;//帳號
    akc_status?:string;//KYC狀態
}
export interface IQueryKycRespModel {
    akc_id: number;//參數代號
    akc_mm_id: number;//參數代號
    mm_account:string
    akc_front: string;//參數名稱
    akc_back: string;//參數描述
    akc_gender: string;//
    akc_personal_id: string;//
    akc_mw_address: string;//
    akc_email: string;//
    akc_bank_account: string;//
    akc_bank_account_name: string;//
    akc_branch: string;//   
    akc_status_name:string;
}

export interface IPutKycReq {
    akc_id?:number;
    mm_id?: number;//覆核人員
    akc_status?: string;//申請狀態:10-待認證、20-認證通過、30-認證駁回、40-認證失敗
}