import { GridSettingModel } from "../../../@entities/shared/grid-setting-model";


export class AnnouncementBoard {
}

export interface IAnnouncementBoardModel {
    ab_id:number;
    ab_title:number;
    ab_content: number;
    ab_image_url : string;
    ab_status: string;
    ab_datetime: Date;
}

export interface IQueryAnnouncementBoard extends GridSettingModel {
    ab_id?: number;
    ab_title?: string;
}

export interface IpostAnnouncementBoard {
    aw_id?:number;
    mm_id?: number;//覆核人員
    aw_mm_id?: number;//儲值人
    aw_status?: string;//申請狀態:0-待會員儲值、1-已匯入儲值金、2-申請駁回、3-匯入失敗
}

export interface IputAnnouncementBoard {
    aw_id?:number;
    mm_id?: number;//覆核人員
    aw_mm_id?: number;//儲值人
    aw_status?: string;//申請狀態:0-待會員儲值、1-已匯入儲值金、2-申請駁回、3-匯入失敗
}