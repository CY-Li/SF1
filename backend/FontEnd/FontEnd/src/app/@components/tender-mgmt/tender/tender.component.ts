import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { RoscaCommonPipe } from '../../../@model/pipes/rosca-common.pipe';
import { IPostWinningModel, IQueryTender, ITenderDetail, ITenderMasterModel } from '../../../@entities/tender-mgmt/tender.model';
import { TenderService } from '../../../@services/tender-mgmt/tender.service';
import { MatDialog } from '@angular/material/dialog';
import { animate, state, style, transition, trigger } from '@angular/animations';
import { TenderAddEditComponent } from './tender-add-edit/tender-add-edit.component';
import { ConfirmDialogCommonComponent } from '../../../@model/dialogs/confirm-dialog-common/confirm-dialog-common.component';

@Component({
  selector: 'app-tender',
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
  templateUrl: './tender.component.html',
  animations: [
    trigger('detailExpand', [
      state('collapsed,void', style({ height: '0px', minHeight: '0' })),
      state('expanded', style({ height: '*' })),
      transition('expanded <=> collapsed', animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)')),
    ]),
  ],
  styleUrl: './tender.component.scss'
})
export class TenderComponent implements AfterViewInit {
  formData: IQueryTender = {};
  // 定義頁面Form表單欄位
  formGroup: FormGroup;

  // 定義頁面Table欄位
  displayedColumns: string[] = [
    "expand",
    // "tm_id",
    "tm_sn",
    "tm_type_name",
    // "tm_bidder",
    "tm_winners",
    // "tm_current_period",
    // "tm_status"
    "tm_win_first_datetime",
    "tm_win_end_datetime"
  ];

    // 定義頁面TableDetail欄位
    displayedDetailColumns: string[] = [
      'action',
      // "td_id",
      // "td_participants",
      "td_sequence",
      "mm_account",
      "td_period",
      // "td_status",
      // "td_update_datetime"
    ];

  myDataSource: MatTableDataSource<ITenderMasterModel> = new MatTableDataSource();
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  totalCount: number = 0;
  mypageIndex: number = 0;
  // row: any;
  expandedElement: ITenderMasterModel | null | undefined;

  constructor(
    private _tenderService: TenderService,//後端API
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
      tm_id: this._formBuilder.control('', Validators.pattern('^[0-9]*$')),
      tm_type: this._formBuilder.control('', Validators.maxLength(1)),
      tm_status: this._formBuilder.control('', Validators.maxLength(1)),
    })
  }

  ngAfterViewInit(): void {
    // this.getMemberList(1, 10);

    this.paginator.page.subscribe((page: PageEvent) => {
      this.getTenderList(page.pageIndex + 1, page.pageSize);
    });
    // this.cdr.detectChanges();
  }

  // 查詢頁面顯示內容
  getTenderList(pageIndex: number, pageSize: number) {
    this.mypageIndex = pageIndex - 1;
    // 手動觸發表單驗證(目前沒找到方法)
    if (this.formGroup.valid) {
      const formData: IQueryTender = this.formGroup.getRawValue();
      formData.pageIndex = pageIndex;
      formData.pageSize = pageSize
      console.log(formData,);
      this._tenderService.getTenderList(formData).subscribe({
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

  openCreateTender(): void {
    const dialogRef = this._dialog.open(TenderAddEditComponent);

    dialogRef.afterClosed().subscribe(result => {
      this.getTenderList(1,10);
    });
  }

  openConfirmDialog(row: ITenderDetail, message: string, funChoose: string): void {
    console.log(row);
    const dialogRef = this._dialog.open(ConfirmDialogCommonComponent, {
      data: { message: message },
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result === true) {
        const requesWinning: IPostWinningModel = {
          mm_id: 1,
          td_id: row.td_id
        };

        this._tenderService.postWinning(requesWinning).subscribe({
          next: (data) => {
            console.log(data);
            if (data && data.result && data.returnStatus == 1) {
              alert("該明細得標成功");
              this.getTenderList(this.mypageIndex + 1, 10);
            } else {
              alert(data.returnMsg);
            }
          }, error: (error) => console.log(error)
        });
      } else {
        console.log(result);
      }
    });
  }
}
