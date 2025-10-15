import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { IQueryKycRespModel } from '../../@entities/authn-mgmt/kyc.model';
import { GridRespModel } from '../../@entities/shared/grid-resp-model';
import { ServiceResultDtoModel } from '../../@entities/shared/service-result-dto-model';
import { Observable } from 'rxjs';

const API_URL = 'http://45.63.127.87/api/Kyc/';
// const API_URL = 'http://localhost:5000/api/Kyc/';

@Injectable({
  providedIn: 'root'
})
export class KycService {

  constructor(private _http: HttpClient) { }

  queryKyc(requestMember: any): Observable<ServiceResultDtoModel<GridRespModel<IQueryKycRespModel>>> {
    let params = new HttpParams();
    Object.keys(requestMember).forEach(key => {
      if (requestMember[key] !== '' && requestMember[key] !== null && requestMember[key] !== undefined) {
        params = params.set(key, requestMember[key]);
      }
    });
    
    return this._http.get<ServiceResultDtoModel<GridRespModel<IQueryKycRespModel>>>(API_URL + 'QueryKycAdmin', {params:params})
  }

  putKyc(requestMember: any): Observable<ServiceResultDtoModel<boolean>> {
    return this._http.put<ServiceResultDtoModel<boolean>>(API_URL, requestMember)
  }
}
