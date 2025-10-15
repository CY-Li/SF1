import { HttpClient, HttpParams } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { GridRespModel } from "../../../@entities/shared/grid-resp-model";
import { ServiceResultDtoModel } from "../../../@entities/shared/service-result-dto-model";
import { AppSettings } from "../../../@shared/app-settings.model";

const API_URL = AppSettings.API_URL + 'HomePicture/'

@Injectable({
  providedIn: 'root'
})
export class AnnouncementService {

  constructor(private _http: HttpClient) { }

  getAnnouncementImages(): Observable<ServiceResultDtoModel<string[]>> {
    return this._http.get<ServiceResultDtoModel<string[]>>(API_URL + 'GetAnnImages');
  }

  putAnnouncementImages(requestModel: any): Observable<ServiceResultDtoModel<boolean>> {
    return this._http.put<ServiceResultDtoModel<boolean>>(API_URL + 'PutAnnFrontPage', requestModel);
  }
}
