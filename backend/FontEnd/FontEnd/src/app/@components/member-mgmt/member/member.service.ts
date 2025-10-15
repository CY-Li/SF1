import { HttpClient, HttpParams } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { GridRespModel } from "../../../@entities/shared/grid-resp-model";
import { ServiceResultDtoModel } from "../../../@entities/shared/service-result-dto-model";
import { AppSettings } from "../../../@shared/app-settings.model";
import { IMember } from "./member.model";

const API_URL = AppSettings.API_URL + 'Members/'

@Injectable({
  providedIn: 'root'
})
export class MemberService {
  
  constructor(private _http: HttpClient) { }
  
  QueryMember(requestMember: any): Observable<ServiceResultDtoModel<GridRespModel<IMember>>> {
    let params = new HttpParams();
    Object.keys(requestMember).forEach(key => {
      if (requestMember[key] !== '' && requestMember[key] !== null && requestMember[key] !== undefined) {
        params = params.set(key, requestMember[key]);
      }
    });

    return this._http.get<ServiceResultDtoModel<GridRespModel<IMember>>>(API_URL + 'QueryMember', {params:params})
  }

  // getMemberList(requestMember:any): Observable<any> {
  //   this._http.post<any>(API_URL + 'GetMemberInfoList',requestMember).subscribe({
  //     next:(data)=>{
  //       if(data != null && data["result"] !=null){
  //         this._memberList = data["result"];
  //       }        
  //     },
  //     error:console.log
      
  //   });

  //   return this._memberList;
  // }

  // getMemberList(requestMember: any): Observable<IMember[]> {
  //   return this._http.post<any>(API_URL + 'GetMemberInfoList', requestMember)
  //     .pipe(
  //       map((data: any) => {
  //         if (data != null && data['result'] != null) {
  //           this._memberList = data['result'];
  //           return this._memberList;
  //         } else {
  //           console.log(data);
  //           throw new Error('Invalid API response');
  //         }
  //       }),
  //       catchError(error => {
  //         throw new Error('API call failed: ' + error.message);
  //       })
  //     );
  // } 
}
