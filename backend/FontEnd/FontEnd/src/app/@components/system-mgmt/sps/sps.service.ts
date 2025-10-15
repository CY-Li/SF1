import { HttpClient, HttpParams } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { GridRespModel } from "../../../@entities/shared/grid-resp-model";
import { ServiceResultDtoModel } from "../../../@entities/shared/service-result-dto-model";
import { AppSettings } from "../../../@shared/app-settings.model";
import { IQuerySpsRespModel } from "./sps.model";

const API_URL = AppSettings.API_URL + 'Sps/'

@Injectable({
  providedIn: 'root'
})
export class SpsService {

  constructor(private _http: HttpClient) { }

  querySpsList(requestMember: any): Observable<ServiceResultDtoModel<GridRespModel<IQuerySpsRespModel>>> {
    let params = new HttpParams();
    Object.keys(requestMember).forEach(key => {
      if (requestMember[key] !== '' && requestMember[key] !== null && requestMember[key] !== undefined) {
        params = params.set(key, requestMember[key]);
      }
    });
    console.log(params);
    return this._http.get<ServiceResultDtoModel<GridRespModel<IQuerySpsRespModel>>>(API_URL + 'QuerySps', { params: params });
  }

  postSps(requestModel: any): Observable<ServiceResultDtoModel<boolean>> {
    return this._http.post<ServiceResultDtoModel<boolean>>(API_URL, requestModel)
  }

  putSps(requestModel: any): Observable<ServiceResultDtoModel<boolean>> {
    return this._http.put<ServiceResultDtoModel<boolean>>(API_URL, requestModel)
  }
}

