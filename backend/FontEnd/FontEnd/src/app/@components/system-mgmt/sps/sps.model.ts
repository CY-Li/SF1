import { GridSettingModel } from "../../../@entities/shared/grid-setting-model";


export interface IQuerySpsReq extends GridSettingModel {
    sps_code?: string;//參數代號
    sps_name?: string;//參數名稱
}
export interface IQuerySpsRespModel {
    sps_id: number;//參數代號
    sps_code: number;//參數代號
    sps_name: string;//參數名稱
    sps_description: string;//參數描述
    sps_parameter01: string;//
    sps_parameter02: string;//
    sps_parameter03: string;//
    sps_parameter04: string;//
    sps_parameter05: string;//
    sps_parameter06: string;//
    sps_parameter07: string;//
    sps_parameter08: string;//
    sps_parameter09: string;//
    sps_parameter10: string;//
}

export interface IPostSpsReqModel extends GridSettingModel {
    mm_id: number;
    sps_id: number;//參數代號
    sps_code: number;//參數代號
    sps_name: string;//參數名稱
    sps_description: string;//參數描述
    sps_parameter01: string;//
    sps_parameter02: string;//
    sps_parameter03: string;//
    sps_parameter04: string;//
    sps_parameter05: string;//
    sps_parameter06: string;//
    sps_parameter07: string;//
    sps_parameter08: string;//
    sps_parameter09: string;//
    sps_parameter10: string;//
}

export interface IPutSpsReqModel extends GridSettingModel {
    mm_id: number;
    sps_id: number;//參數代號
    sps_code: number;//參數代號
    sps_name: string;//參數名稱
    sps_description: string;//參數描述
    sps_parameter01: string;//
    sps_parameter02: string;//
    sps_parameter03: string;//
    sps_parameter04: string;//
    sps_parameter05: string;//
    sps_parameter06: string;//
    sps_parameter07: string;//
    sps_parameter08: string;//
    sps_parameter09: string;//
    sps_parameter10: string;//
}