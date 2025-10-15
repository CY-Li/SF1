import { GridSettingModel } from "../shared/grid-setting-model";

export interface IMemberBalanceModel {
    mb_id: number;
    mb_mm_id: number;//儲值人
    mb_payee_mm_id: string;//收款人
    mb_tm_id: number;//標案ID
    mb_td_id: number;//標案明細ID
    // mb_income_type: string;//點數流向:I-收入，O支出(寫入時全大寫)
    mb_income_type_name: string;
    // mb_tr_type:string;
    mb_tr_type_name: string;
    // mb_type: string;//11-下標時扣除押金(儲值)、12-下標時扣除額(儲值)、13-下層下標時給的紅利(紅利)、14-成組後扣除額(未死會:儲值、死會:紀錄10000(不進mb))、15-中標時新增紅利額度(紅利:包括押金)、16-排程處理中標時歸還押金(紅利>儲值)、17-排程處理中標時新增儲值(紅利>儲值)、18-排程處理下層下標時給的紅利(紅利>儲值)、19-儲值點數、20-領取點數
    mb_type_name: string;
    // mb_points_type: number;//點數類型
    mb_points_type_name: string;
    mb_change_points: number;//點數異動額
    mb_points: number;//點數異動後額度
    mb_create_datetime: Date;
}

export interface IQueryMemberBalance extends GridSettingModel {
    mm_account?: string;//
    mb_points_type?:string;
}