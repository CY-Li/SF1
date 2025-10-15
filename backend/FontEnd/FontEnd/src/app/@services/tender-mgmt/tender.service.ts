import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ServiceResultDtoModel } from '../../@entities/shared/service-result-dto-model';
import { GridRespModel } from '../../@entities/shared/grid-resp-model';
import { ITenderMasterModel } from '../../@entities/tender-mgmt/tender.model';

const API_URL = 'http://45.63.127.87/api/';
// const API_URL = 'http://localhost:5000/api/';

@Injectable({
  providedIn: 'root'
})
export class TenderService {

  constructor(private _http: HttpClient) { }

  // getTenderList(requestMember: any): Observable<ServiceResultDtoModel<GridRespModel<ITenderMasterModel>>> {
  //   return this._http.get<ServiceResultDtoModel<GridRespModel<ITenderMasterModel>>>(API_URL + 'Tender/QueryTenderAdmin', requestMember)
  // }
  getTenderList(requestMember: any): Observable<ServiceResultDtoModel<GridRespModel<ITenderMasterModel>>> {
    let params = new HttpParams();
    Object.keys(requestMember).forEach(key => {
      if (requestMember[key] !== '' && requestMember[key] !== null && requestMember[key] !== undefined) {
        params = params.set(key, requestMember[key]);
      }
    });
    console.log(params);
    return this._http.get<ServiceResultDtoModel<GridRespModel<ITenderMasterModel>>>(API_URL + 'Tender/QueryTenderAdmin',{params:params});
  }
  postTender(requestMember: any): Observable<ServiceResultDtoModel<boolean>> {
    return this._http.post<ServiceResultDtoModel<boolean>>(API_URL + 'Tender', requestMember)
  }

  postWinning(requestMember: any): Observable<ServiceResultDtoModel<boolean>> {
    return this._http.post<ServiceResultDtoModel<boolean>>(API_URL + 'Tender/WinningTender', requestMember)
  }
}
