import { CommonModule } from '@angular/common';
import { Component, ViewChild, Pipe, AfterViewInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatDialog } from '@angular/material/dialog';
import { animate, state, style, transition, trigger } from '@angular/animations';
import { MatSort, MatSortModule, Sort } from '@angular/material/sort';
import { catchError, merge, startWith, switchMap, Observable, of as observableOf, map, tap } from 'rxjs';
import { ConfirmDialogCommonComponent } from '../../../@model/dialogs/confirm-dialog-common/confirm-dialog-common.component';
import { IAnnouncementBoardModel, IQueryAnnouncementBoard } from './ann-board.model';
import { AnnBoardService } from './ann-board.service';
import { AnnBoardAddEditComponent } from './ann-board-add-edit/ann-board-add-edit.component';

@Component({
  selector: 'app-announcement-board',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatIconModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    // RoscaCommonPipe,
  ],
  templateUrl: './announcement-board.component.html',
  animations: [
    trigger('detailExpand', [
      state('collapsed,void', style({ height: '0px', minHeight: '0' })),
      state('expanded', style({ height: '*' })),
      transition('expanded <=> collapsed', animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)')),
    ]),
  ],
  styleUrl: './announcement-board.component.scss'
})
export class AnnouncementBoardComponent implements AfterViewInit {
  //Start Section 1: Form Setup=======================================================================================

  // 傳送給伺服器接收參數
  formData: IQueryAnnouncementBoard = {};
  // 定義頁面Form表單用
  formGroup: FormGroup;
  // 初始化Form表單條件用
  initFormGroup() {
    // 定義頁面Form表單查詢條件
    return this.formGroup = this._formBuilder.group({
      ab_id: this._formBuilder.control('', Validators.pattern('^\\d+$')),
      ab_title: this._formBuilder.control('', Validators.maxLength(30))
    })
  }
  //End Section 1: Form Setup=======================================================================================

  //Start Section 2: Table Setup=======================================================================================

  // tableData: MatTableDataInitial = new MatTableDataInitial();
  // @ViewChild(MatPaginator, { static: true }) set paginator(paginator: MatPaginator) {
  //   this.tableData.paginator = paginator; // 將 paginator 賦值給 tableData
  // }

  // Table分頁功能，可用來訂閱頁面分頁改變狀態、取得當下分頁頁數
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  // Table排序功能，可用來訂閱頁面排序改變狀態、取得當下排序資訊
  @ViewChild(MatSort) sort!: MatSort;
  pageIndex: number = 0; // 頁面的分頁頁數，因this.paginator.pageIndex的數字為0開始所以操作都要加1傳到API才會取得正確頁數
  pageSize: number = 10; // 頁面的分頁大小
  pageSort: string = ''; // 頁面的排序欄位
  pageSortDirection: string = ''; // 頁面的排序順序
  totalCount: number = 0; // API取得的剩餘資料總頁數
  // Table資料放置位置，並將欄位定義Interface匯入
  dataSource: MatTableDataSource<IAnnouncementBoardModel> = new MatTableDataSource();
  expandedElement: any | null | undefined; // Table明細是否展開所需bool
  isLoadingResults = true; //向API取得資料時，顯示選轉圈圈的狀態保存用
  isRateLimitReached = false;

  // 定義頁面Table欄位
  displayedColumns: string[] = [
    'action',
    'ab_id',
    'ab_title',
    'ab_content',
    // 'ab_status',
    // 'ab_datetime'
  ];

  //End Section 2: Table Setup=======================================================================================

  //Start Section 3: Component Setup=======================================================================================

  constructor(
    private _formBuilder: FormBuilder, // 表單建立用
    private _dialog: MatDialog, // 開啟dialog，新增、編輯用，檢視目前用master-detail使用
    private _service: AnnBoardService, // 後端API服務呼叫用
    // private cdr: ChangeDetectorRef // 引入 ChangeDetectorRef 避免 NG0100 錯誤。(暫時沒用)
  ) {
    this.formGroup = this.initFormGroup(); // 頁面表單條件初始化用
  }

  ngAfterViewInit(): void {
    console.log(this.paginator.pageIndex);
    merge(this.sort.sortChange, this.paginator.page)
      .pipe(
        tap(() => {
          console.log('tap', this.paginator.pageIndex);
          this.queryFrontPageList(this.paginator.pageIndex, this.paginator.pageSize, this.sort.active, this.sort.direction);
        })
      ).subscribe(); // 頁面分頁/排序訂閱，監視分頁/排序事件

    // this.paginator.page.subscribe((page: PageEvent) => {
    //   this.queryFrontPageList(page.pageIndex + 1, page.pageSize);
    // }); // 頁面分頁訂閱，監視分頁事件
    // this.sort.sortChange.subscribe(() => (this.paginator.pageIndex = 0)); // 頁面排序訂閱，監視排序事件

    // this.getMemberList(1, 10);
    // this.cdr.detectChanges();
  }

  //End Section 3: Component Setup=======================================================================================

  //Start Section 4: Component Mathod Setup=======================================================================================

  // 查詢頁面顯示內容
  queryFrontPageList(pageIndex: number, pageSize: number, sortColumn: string, sortDirection: string) {
    this.pageIndex = pageIndex;
    this.pageSize = pageSize;
    this.pageSort = sortColumn;
    this.pageSortDirection = sortDirection;
    console.log('1:' + pageIndex, '2:' + pageSize, '3:' + sortColumn, '4:' + sortDirection)
    // 手動觸發表單驗證(目前沒找到方法)
    if (this.formGroup.valid) {
      const formData: IQueryAnnouncementBoard = this.formGroup.getRawValue();
      formData.pageIndex = pageIndex + 1; // 因this.paginator.pageIndex的數字為0開始所以操作都要加1傳到API才會取得正確頁數
      formData.pageSize = pageSize;
      formData.pageSort = sortColumn;
      formData.pageSortDirection = sortDirection;
      console.log('formData:', formData);
      this._service.QueryAnnouncementBoard(formData).subscribe({
        next: (data) => {
          console.log(data);
          if (data && data.result && data.returnStatus == 1) {
            this.totalCount = data.result?.gridTotalCount;
            this.dataSource.data = data.result?.gridRespResult;
            // this.dataSource = new MatTableDataSource(data.result?.gridRESPResult);
          } else {
            console.log(data);
          }
        }, error: (error) => {
          alert(error);
          console.log(error)
        }
      });
    } else {
      alert('請輸入正確資料');
    }
  }

  // 新增、編輯都開啟Dialog
  openAddEditForm(isView: boolean, formData?: any,) {
    const dialogRef = this._dialog.open(AnnBoardAddEditComponent, {
      data: { isView: isView, formData: formData },
    });

    // 監聽Dialog關閉事件
    dialogRef.afterClosed().subscribe({
      next: (val) => {
        console.log(val);
        // 新增/更新，成功時才更新目前頁面Table
        if (val) {
          this.queryFrontPageList(0, this.pageSize, this.pageSort, this.pageSortDirection);
        }
      },
    });
  }

  onDelete(id: number, message: string) {
    const dialogRef = this._dialog.open(ConfirmDialogCommonComponent, {
      data: { message: message },
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result === true) {
        // this._spsService.queryFrontPageList(id).subscribe({
        //   next: (data) => {
        //     if (data && data.result && data.returnStatus == 1) {
        //       alert(data.returnMsg);
        //       this.queryFrontPageList(0, this.pageSize, this.pageSort, this.pageSortDirection);
        //     } else {
        //       alert(data.returnMsg);
        //       console.log(data);
        //     }
        //   },
        //   error: (error) => console.error(error)
        // });
      }
    });
  }

  //End Section 4: Component Mathod Setup=======================================================================================

  //Start Section 5: Component unique Mathod Setup=======================================================================================
  //Start Section 5: Component unique Mathod Setup=======================================================================================
}
