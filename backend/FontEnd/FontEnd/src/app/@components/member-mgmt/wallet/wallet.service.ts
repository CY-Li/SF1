import { HttpClient, HttpParams } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { GridRespModel } from "../../../@entities/shared/grid-resp-model";
import { ServiceResultDtoModel } from "../../../@entities/shared/service-result-dto-model";
import { AppSettings } from "../../../@shared/app-settings.model";
import { IQueryWalletRespModel } from "./wallet.model";


const API_URL = AppSettings.API_URL + 'Wallet/'

@Injectable({
  providedIn: 'root'
})
export class WalletService {

  constructor(private _http: HttpClient) { }

    QueryWallet(requestMember: any): Observable<ServiceResultDtoModel<GridRespModel<IQueryWalletRespModel>>> {
      let params = new HttpParams();
      Object.keys(requestMember).forEach(key => {
        if (requestMember[key] !== '' && requestMember[key] !== null && requestMember[key] !== undefined) {
          params = params.set(key, requestMember[key]);
        }
      });
  
      return this._http.get<ServiceResultDtoModel<GridRespModel<IQueryWalletRespModel>>>(API_URL + 'QueryWallet', {params:params})
    }
}
