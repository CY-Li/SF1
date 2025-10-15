import { HttpClient, HttpParams } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { GridRespModel } from "../../../@entities/shared/grid-resp-model";
import { ServiceResultDtoModel } from "../../../@entities/shared/service-result-dto-model";
import { AppSettings } from "../../../@shared/app-settings.model";
import { IAnnouncementBoardModel } from "./ann-board.model";


const API_URL = AppSettings.API_URL + 'Announcement/'

@Injectable({
  providedIn: 'root'
})
export class AnnBoardService {

  constructor(private _http: HttpClient) { }

  QueryAnnouncementBoard(requestMember: any): Observable<ServiceResultDtoModel<GridRespModel<IAnnouncementBoardModel>>> {
    let params = new HttpParams();
    Object.keys(requestMember).forEach(key => {
      if (requestMember[key] !== '' && requestMember[key] !== null && requestMember[key] !== undefined) {
        params = params.set(key, requestMember[key]);
      }
    });
    
    return this._http.get<ServiceResultDtoModel<GridRespModel<IAnnouncementBoardModel>>>(API_URL + 'QueryAnnBoardAdmin', {params:params})
  }

  postAnnouncementBoard(requestModel: any): Observable<ServiceResultDtoModel<boolean>> {
    return this._http.post<ServiceResultDtoModel<boolean>>(API_URL, requestModel);
  }

  putAnnouncementBoard(requestModel: any): Observable<ServiceResultDtoModel<boolean>> {
    return this._http.put<ServiceResultDtoModel<boolean>>(API_URL, requestModel);
  }
}
