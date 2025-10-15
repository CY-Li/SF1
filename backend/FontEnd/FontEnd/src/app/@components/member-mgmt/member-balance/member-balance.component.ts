import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { IMemberBalanceModel, IQueryMemberBalance } from '../../../@entities/member-mgmt/member-balance.model';
import { MatDialog } from '@angular/material/dialog';
import { MemberBalanceService } from '../../../@services/member-mgmt/member-balance.service';
import { RoscaCommonPipe } from '../../../@model/pipes/rosca-common.pipe';
import { MatSelectModule } from '@angular/material/select';

@Component({
  selector: 'app-member-balance',
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
  templateUrl: './member-balance.component.html',
  styleUrl: './member-balance.component.scss'
})
export class MemberBalanceComponent {
  formData: IQueryMemberBalance = {};
  // 定義頁面Form表單欄位
  formGroup: FormGroup;
  // formGroup: FormGroup = new FormGroup({
  //   mm_account: new FormControl(''),
  //   mm_name: new FormControl(''),
  //   mm_invite_code: new FormControl('')
  // });

  // 定義頁面Table欄位
  displayedColumns: string[] = [
    // 'action',
    'mb_id',
    'mb_mm_id',
    'mb_payee_mm_id',
    'mb_tm_id',
    'mb_td_id',
    // 'mb_income_type',
    'mb_income_type_name',
    // 'mb_tr_type',
    'mb_tr_type_name',
    // 'mb_type',
    'mb_type_name',
    // 'mb_points_type',
    'mb_points_type_name',
    'mb_change_points',
    'mb_points',
    'mb_create_datetime'
  ];

  //定義Table來源型態
  myDataSource: MatTableDataSource<IMemberBalanceModel> = new MatTableDataSource();

  // 定義分頁
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  totalCount: number = 0;
  mypageIndex: number = 0

  constructor(
    private _MemberBalanceService: MemberBalanceService,//後端API
    private _formBuilder: FormBuilder,//表單建立
    private _dialog: MatDialog,//開啟dialog
    private cdr: ChangeDetectorRef // 引入 ChangeDetectorRef 避免 NG0100 錯誤。(暫時沒用)
  ) {
    // 取得Table資料
    // this.formData.pageIndex = 0;
    // this.formData.pageSize = 10;
    // this.getMemberList(formData);
    // this.myDataSource = new MatTableDataSource(rowData);

    // 定義頁面查詢條件
    this.formGroup = this._formBuilder.group({
      mm_account: this._formBuilder.control(0,  Validators.pattern('^09\\d{8}$')),
      mb_points_type: this._formBuilder.control(0,  Validators.pattern('^(?:11|12|13|14|15|16)?$'))
    })
  }

    // 頁面載入完成後執行
    ngAfterViewInit(): void {
      // this.getMemberList(1, 10);
  
      this.paginator.page.subscribe((page: PageEvent) => {
        this.getMemberBalanceList(page.pageIndex + 1, page.pageSize);
      });
      // this.cdr.detectChanges();
    }
  
    // 查詢頁面顯示內容
    getMemberBalanceList(pageIndex: number, pageSize: number) {
      this.mypageIndex = pageIndex - 1;
      // 手動觸發表單驗證(目前沒找到方法)
      if (this.formGroup.valid) {
        const formData: IQueryMemberBalance = this.formGroup.getRawValue();
        formData.pageIndex = pageIndex;
        formData.pageSize = pageSize
        console.log(formData,);
        this._MemberBalanceService.QueryMemberBalanceUser(formData).subscribe({
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
}
