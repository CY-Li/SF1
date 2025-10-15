import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ServiceResultDtoModel } from '../../@entities/shared/service-result-dto-model';
import { GridRespModel } from '../../@entities/shared/grid-resp-model';
import { IApplyDepositModel } from '../../@entities/member-mgmt/apply-deposit-model';
import { Observable } from 'rxjs';

const API_URL = 'http://45.63.127.87/api/ApplyDeposit/';
// const API_URL = 'http://localhost:5000/api/ApplyDeposit/';

@Injectable({
  providedIn: 'root'
})
export class ApplyDepositService {

  constructor(private _http: HttpClient) { }

  QueryApplyDeposit(requestMember: any): Observable<ServiceResultDtoModel<GridRespModel<IApplyDepositModel>>> {
    let params = new HttpParams();
    Object.keys(requestMember).forEach(key => {
      if (requestMember[key] !== '' && requestMember[key] !== null && requestMember[key] !== undefined) {
        params = params.set(key, requestMember[key]);
      }
    });
    
    return this._http.get<ServiceResultDtoModel<GridRespModel<IApplyDepositModel>>>(API_URL + 'QueryApplyDepositAdmin', {params:params})
  }

  putApplyDeposit(requestMember: any): Observable<ServiceResultDtoModel<boolean>> {
    return this._http.put<ServiceResultDtoModel<boolean>>(API_URL, requestMember)
  }
}
