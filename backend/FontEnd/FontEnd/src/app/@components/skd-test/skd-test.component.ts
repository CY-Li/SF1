import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { HttpClient } from '@angular/common/http';

const API_URL = 'http://45.63.127.87/api/Schedule/';
// const API_URL = 'http://localhost:5000/api/Schedule/';

@Component({
  selector: 'app-skd-test',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule
  ],
  templateUrl: './skd-test.component.html',
  styleUrl: './skd-test.component.scss'
})
export class SkdTestComponent {
  constructor(private _http: HttpClient) {

  }
  
  biddingSchedule() {
    this._http.post(API_URL + 'BiddingSchedule', null).subscribe({
      next: (data) => {
        console.log(data);
        alert('紅利派發執行結束');
      }, error: (error) => console.log(error)
    });
  }

  settlementRewardSchedule() {
    this._http.post(API_URL + 'SettlementRewardSchedule', null).subscribe({
      next: (data) => {
        console.log(data);
        alert('清算紅利執行結束');
      }, error: (error) => console.log(error)
    });
  }

  settlementPeaceSchedule() {
    this._http.post(API_URL + 'SettlementPeaceSchedule', null).subscribe({
      next: (data) => {
        console.log(data);
        alert('清算平安點數執行結束');
      }, error: (error) => console.log(error)
    });
  }

  groupDebitSchedule() {
    this._http.post(API_URL + 'GroupDebitSchedule', null).subscribe({
      next: (data) => {
        console.log(data);
        alert('成組排成執行結束');
      }, error: (error) => console.log(error)
    });
  }

  pendingPaymentSchedule() {
    this._http.post(API_URL + 'PendingPaymentSchedule', null).subscribe({
      next: (data) => {
        console.log(data);
        alert('待付款排成執行結束');
      }, error: (error) => console.log(error)
    });
  }
}
