import { GridSettingModel } from "../shared/grid-setting-model";

export interface ITenderMasterModel {
    tm_id: number;//標案代號
    tm_sn: number;//成組序號
    // tm_type: string;//標會種類:1-一般玩法(散會:可以選下標數)、2-1人玩法、3-2人玩法、4-4人玩法
    tm_type_name: string;
    // tm_bidder: string;//下標人會用|串接，EX:1|2|3
    tm_winners: string;//得獎者會用|串接，EX:1|2|3
    // tm_current_period:number;//目前期數(成組時，當下期數，剛寫入就是1)
    tm_status: string;//是否成組:0-未成組,1-成組,2-結束
    tm_win_first_datetime: Date;//第一次開標時間
    tm_win_end_datetime: Date;//此組最後一次開標時間
    tenderDetail?: ITenderDetail[];
}

export interface ITenderDetail {
    td_id: number;
    // td_participants: number;//參加者mm_id
    td_sequence: number;//標案明細序列(用來方便做查找)
    td_period: number;//該標中標期數
    // td_status: string;//下標狀態
    // td_update_datetime: Date;
    mm_account: string;//會員帳號
}

export interface IQueryTender extends GridSettingModel {
    mm_account?: string;//會員帳號
    roscam_name?: string;//標會名稱
    roscam_type?: string//標會種類:1-一般玩法、2-2人玩法、3-4人玩法、4-6人玩法、5-12人玩法
}

//不用了
export interface IPostTender {
    mm_id: number;//起標人
    roscam_name: string;//標會名稱
    roscam_type: string;//標會種類
}

export interface IPostWinningModel {
    mm_id: number;
    td_id: number;
}