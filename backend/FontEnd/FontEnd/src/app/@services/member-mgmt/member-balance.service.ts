import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ServiceResultDtoModel } from '../../@entities/shared/service-result-dto-model';
import { GridRespModel } from '../../@entities/shared/grid-resp-model';
import { IMemberBalanceModel } from '../../@entities/member-mgmt/member-balance.model';

const API_URL = 'http://45.63.127.87/api/Balances/';
// const API_URL = 'http://localhost:5000/api/Balances/';

@Injectable({
  providedIn: 'root'
})
export class MemberBalanceService {

  constructor(private _http: HttpClient) { }

  QueryMemberBalanceUser(requestMember: any): Observable<ServiceResultDtoModel<GridRespModel<IMemberBalanceModel>>> {
    let params = new HttpParams();
    Object.keys(requestMember).forEach(key => {
      if (requestMember[key] !== '' && requestMember[key] !== null && requestMember[key] !== undefined) {
        params = params.set(key, requestMember[key]);
      }
    });
    
    return this._http.get<ServiceResultDtoModel<GridRespModel<IMemberBalanceModel>>>(API_URL + 'QueryMemberBalanceAdmin', {params:params})
  }
}
