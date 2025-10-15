import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { IApplyDepositModel, IQueryApplyDeposit, IputApplyDepositModel } from '../../../@entities/member-mgmt/apply-deposit-model';
import { MatDialog } from '@angular/material/dialog';
import { ApplyDepositService } from '../../../@services/member-mgmt/apply-deposit.service';
import { RoscaCommonPipe } from '../../../@model/pipes/rosca-common.pipe';
import { MatSelectModule } from '@angular/material/select';
import { ConfirmDialogCommonComponent } from '../../../@model/dialogs/confirm-dialog-common/confirm-dialog-common.component';

@Component({
  selector: 'app-apply-deposit',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatIconModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    RoscaCommonPipe,
    MatSelectModule
  ],
  templateUrl: './apply-deposit.component.html',
  styleUrl: './apply-deposit.component.scss'
})
export class ApplyDepositComponent implements AfterViewInit {
  formData: IQueryApplyDeposit = {};
  // 定義頁面Form表單欄位
  formGroup: FormGroup;

  // 定義頁面Table欄位
  displayedColumns: string[] = [
    'action',
    'ad_id',
    'mm_account',
    // 'ad_mm_id',
    'ad_amount',
    'ad_key',
    // 'ad_picture',
    'ad_status',
    'ad_url',
    'ad_creat_datetime'
    
  ];

  myDataSource: MatTableDataSource<IApplyDepositModel> = new MatTableDataSource();
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  totalCount: number = 0;
  mypageIndex: number = 0;
  row: any;

  constructor(
    private _applyDepositService: ApplyDepositService,//後端API
    private _formBuilder: FormBuilder,//表單建立
    private _dialog: MatDialog//開啟dialog
    // private cdr: ChangeDetectorRef // 引入 ChangeDetectorRef 避免 NG0100 錯誤。(暫時沒用)
  ) {
    // 取得Table資料
    // this.formData.pageIndex = 0;
    // this.formData.pageSize = 10;
    // this.getMemberList(formData);
    // this.myDataSource = new MatTableDataSource(rowData);

    // 定義頁面查詢條件
    this.formGroup = this._formBuilder.group({
      mm_account: this._formBuilder.control('', Validators.pattern('^09\\d{8}$')),
      mm_name: this._formBuilder.control('', Validators.maxLength(30)),
      ad_status: this._formBuilder.control(0,  Validators.pattern('^(?:10|11|12|13)?$'))
    })
  }

  // 頁面載入完成後執行
  ngAfterViewInit(): void {
    // this.getMemberList(1, 10);

    this.paginator.page.subscribe((page: PageEvent) => {
      this.getApplyDepositLis(page.pageIndex + 1, page.pageSize);
    });
    // this.cdr.detectChanges();
  }

  // 查詢頁面顯示內容
  getApplyDepositLis(pageIndex: number, pageSize: number) {
    this.mypageIndex = pageIndex - 1;
    // 手動觸發表單驗證(目前沒找到方法)
    if (this.formGroup.valid) {
      const formData: IQueryApplyDeposit = this.formGroup.getRawValue();
      formData.pageIndex = pageIndex;
      formData.pageSize = pageSize
      console.log(formData,);
      this._applyDepositService.QueryApplyDeposit(formData).subscribe({
        next: (data) => {
          console.log('data',data);
          if (data && data.result && data.returnStatus == 1) {
            this.totalCount = data.result?.gridTotalCount;
            this.myDataSource.data = data.result?.gridRespResult;
            // this.myDataSource = new MatTableDataSource(data.result?.gridRESPResult);
          } else {
            console.log(data);
          }
        }, error: (error) => console.log(error)
      });
    } else {
      alert('請輸入正確資料');
    }
  }

  openConfirmDialog(row: IApplyDepositModel, message: string, funChoose: string): void {
    const dialogRef = this._dialog.open(ConfirmDialogCommonComponent, {
      data: { message: message },
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result === true) {
        const requesrApplyDeposit: IputApplyDepositModel = {};
        requesrApplyDeposit.ad_id = row.ad_id;
        requesrApplyDeposit.mm_id = 1;
        requesrApplyDeposit.ad_mm_id = row.ad_mm_id;
        if (funChoose === "OK") {
          requesrApplyDeposit.ad_status = "11";
        } else {
          requesrApplyDeposit.ad_status = "12";
        }

        this._applyDepositService.putApplyDeposit(requesrApplyDeposit).subscribe({
          next: (data) => {
            console.log(data);
            if (data && data.result && data.returnStatus == 1) {
              alert("覆核成功");
              this.getApplyDepositLis(this.mypageIndex + 1, 10);
            } else {
              console.log(data);
            }
          }, error: (error) => console.log(error)
        });
      } else {
        console.log(result);
      }
    });
  }
  // clickMethod(row: any) {
  //   console.log(row);
  //   if (confirm("Are you sure to delete ")) {
  //     console.log("Implement delete functionality here");
  //   }
  // }
}
