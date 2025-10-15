import { GridSettingModel } from "../../../@entities/shared/grid-setting-model";

export interface IMember {
    mm_id?: number;//會員ID
    mm_account?: string;//會員帳號
    mm_name?: string;//會員名稱
    mm_introduce_user?: string;//介紹人
    // mm_invite_code?: string;//邀請碼
    mm_invite_user?: string;//邀請人
    mm_level?:string;
    // mm_role_type?: string;//角色
    mm_status?: string;//狀態
    mm_kyc?:string;//KYC狀態
    mm_mw_address?:string;//虛擬錢包地址
    mm_creat_member?: string;
    mm_creat_datetime?: string;//加入時間
}

export interface IQueryMember extends GridSettingModel {
    mm_account?: string;//會員帳號
    mm_name?: string;//會員名稱
    // mm_introduce_code?: number;//介紹人
    mm_introduce_user?: string;//介紹人
    // mm_invite_code?: number;//邀請碼
    mm_invite_user?: string;//邀請碼
}
