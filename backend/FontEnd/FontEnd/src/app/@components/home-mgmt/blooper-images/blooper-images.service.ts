import { Injectable } from '@angular/core';
import { AppSettings } from '../../../@shared/app-settings.model';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ServiceResultDtoModel } from '../../../@entities/shared/service-result-dto-model';

const API_URL = AppSettings.API_URL + 'HomePicture/'


@Injectable({
  providedIn: 'root'
})
export class BlooperImagesService {

    constructor(private _http: HttpClient) { }
  
    getBlooperImages(): Observable<ServiceResultDtoModel<string[]>> {
      return this._http.get<ServiceResultDtoModel<string[]>>(API_URL + 'GetBlooperImages');
    }
  
    putBlooperImages(requestModel: any): Observable<ServiceResultDtoModel<boolean>> {
      return this._http.put<ServiceResultDtoModel<boolean>>(API_URL + 'PutBlooperFrontPage', requestModel);
    }
}
