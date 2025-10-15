import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { IApplyWithdrawModel } from '../../@entities/member-mgmt/apply-withdraw.model';
import { GridRespModel } from '../../@entities/shared/grid-resp-model';
import { ServiceResultDtoModel } from '../../@entities/shared/service-result-dto-model';
import { Observable } from 'rxjs';

const API_URL = 'http://45.63.127.87/api/ApplyWithdraw/';
// const API_URL = 'http://localhost:5000/api/ApplyWithdraw/';

@Injectable({
  providedIn: 'root'
})
export class ApplyWithdrawService {

  constructor(private _http: HttpClient) { }

  QueryApplyWithdraw(requestMember: any): Observable<ServiceResultDtoModel<GridRespModel<IApplyWithdrawModel>>> {
    let params = new HttpParams();
    Object.keys(requestMember).forEach(key => {
      if (requestMember[key] !== '' && requestMember[key] !== null && requestMember[key] !== undefined) {
        params = params.set(key, requestMember[key]);
      }
    });
    
    return this._http.get<ServiceResultDtoModel<GridRespModel<IApplyWithdrawModel>>>(API_URL + 'QueryApplyWithdrawAdmin', {params:params})
  }

  putApplyWithdraw(requestMember: any): Observable<ServiceResultDtoModel<boolean>> {
    return this._http.put<ServiceResultDtoModel<boolean>>(API_URL, requestMember)
  }
}
