import { Component, ViewChild } from '@angular/core';
import { IPutKycReq, IQueryKycReq, IQueryKycRespModel } from '../../../@entities/authn-mgmt/kyc.model';
import { MatFormFieldModule } from '@angular/material/form-field';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormBuilder, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { RoscaCommonPipe } from '../../../@model/pipes/rosca-common.pipe';
import { KycService } from '../../../@services/authn-mgmt/kyc.service';
import { ConfirmDialogCommonComponent } from '../../../@model/dialogs/confirm-dialog-common/confirm-dialog-common.component';
import { trigger, state, style, transition, animate } from '@angular/animations';

@Component({
  selector: 'app-kyc',
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
  templateUrl: './kyc.component.html',
  styleUrl: './kyc.component.scss'
})
export class KycComponent {
  formData: IQueryKycReq = {};
  // 定義頁面Form表單欄位
  formGroup: FormGroup;

  // 定義頁面Table欄位
  displayedColumns: string[] = [
    'action',
    'akc_id',
    // 'akc_mm_id',
    'mm_account',
    // 'akc_front',
    // 'akc_back',
    'akc_gender',
    'akc_personal_id',
    'akc_mw_address',
    'akc_email',
    'akc_bank_account',
    'akc_bank_account_name',
    'akc_branch',
    'akc_status_name'
  ];
  // expandedElement: IQueryKycRespModel | null | undefined;
  myDataSource: MatTableDataSource<IQueryKycRespModel> = new MatTableDataSource();
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  totalCount: number = 0;
  mypageIndex: number = 0;
  row: any;

  constructor(
    private _kycService: KycService,//後端API
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
      akc_id: this._formBuilder.control(0,  Validators.pattern('/^[0-9]*$/')),
      mm_account: this._formBuilder.control('', Validators.pattern('^09\\d{8}$')),
      akc_status: this._formBuilder.control('', Validators.pattern('^(?:10|20|30)?$'))
    })
  }

  // 頁面載入完成後執行
  ngAfterViewInit(): void {
    // this.getMemberList(1, 10);

    this.paginator.page.subscribe((page: PageEvent) => {
      this.queryKyc(page.pageIndex + 1, page.pageSize);
    });
    // this.cdr.detectChanges();
  }

  // 查詢頁面顯示內容
  queryKyc(pageIndex: number, pageSize: number) {
    this.mypageIndex = pageIndex - 1;
    // 手動觸發表單驗證(目前沒找到方法)
    if (this.formGroup.valid) {
      const formData: IQueryKycReq = this.formGroup.getRawValue();
      formData.pageIndex = pageIndex;
      formData.pageSize = pageSize
      console.log(formData,);
      this._kycService.queryKyc(formData).subscribe({
        next: (data) => {
          console.log(data);
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

  openConfirmDialog(row: IQueryKycRespModel, message: string, funChoose: string): void {
    const dialogRef = this._dialog.open(ConfirmDialogCommonComponent, {
      data: { message: message },
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result === true) {
        const requesrKyc: IPutKycReq = {};
        requesrKyc.akc_id = row.akc_id;
        requesrKyc.mm_id = 1;
        if (funChoose === "OK") {
          requesrKyc.akc_status = "20";
        } else {
          requesrKyc.akc_status = "30";
        }

        this._kycService.putKyc(requesrKyc).subscribe({
          next: (data) => {
            console.log(data);
            if (data && data.result && data.returnStatus == 1) {
              alert("覆核成功");
              this.queryKyc(this.mypageIndex + 1, 10);
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
}
