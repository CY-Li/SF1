import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogActions, MatDialogContent, MatDialogRef, MatDialogTitle } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { WalletService } from '../wallet.service';


@Component({
  selector: 'app-wallet-add-edit',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatIconModule,
    MatDialogActions,
    MatDialogTitle,
    MatDialogContent
  ],
  templateUrl: './wallet-add-edit.component.html',
  styleUrl: './wallet-add-edit.component.scss'
})
export class WalletAddEditComponent {
  //Start Section 1: Form Setup=======================================================================================
  // 定義頁面Form表單用
  addEditFormGroup: FormGroup;
  // 初始化Form表單條件用
  initFormGroup() {
    // 定義頁面Form表單查詢條件
    return this._formBuilder.group({
      mw_id: '',
      mm_account: '',
      mm_name: '',
      mw_stored: '',
      mw_accumulation:'',
      mw_reward:'',
      mw_mall: '',
    });
  }

  // currentItem: any = null;
  // editFormData: any = {};
  //End Section 1: Form Setup=======================================================================================

  //Start Section 2: Table Setup=======================================================================================
  //End Section 2: Table Setup=======================================================================================

  //Start Section 3: Component Setup=======================================================================================

  isView: boolean = false; //判斷頁面是否為檢視，檢視不可有一些操作例如送出表單
  isEditing: boolean = false; //如果是編輯用來控制欄位不可修改
  isReadonly: boolean = true; //用來判斷頁面新增、修改時是否不可輸入(系統自帶)

  constructor(
    private _formBuilder: FormBuilder, // 表單建立用
    private _dialogRef: MatDialogRef<WalletAddEditComponent>, // 關閉dialog，新增、編輯用
    @Inject(MAT_DIALOG_DATA) public data: any, // 主頁面傳入資料用
    private _service: WalletService // 後端API服務呼叫用
  ) {
    this.isView = this.data.isView;
    if (data.formData) this.isEditing = true; //有資料則是編輯
    this.addEditFormGroup = this.initFormGroup() // 頁面表單條件初始化用

  }

  ngOnInit(): void {
    //將傳入該Dialog資料放入表單中
    this.addEditFormGroup.patchValue(this.data.formData);
    if (this.isView) this.addEditFormGroup.disable();
  }

  //End Section 3: Component Setup=======================================================================================
  //Start Section 4: Component Mathod Setup=======================================================================================

  // 表單送出呼叫用
  // onFormSubmit(): void {
  //   // 驗證表單條件是否正確
  //   if (this.addEditFormGroup.valid) {

  //     // 判斷有資料則是編輯
  //     if (this.data.formData) {
  //       // 表單資料放進呼叫API要傳的參數
  //       const editFormData: IPutSpsReqModel = this.addEditFormGroup.getRawValue();

  //       console.log('更新:', editFormData)

  //       // 更新(成功在主動關Dialog)
  //       this._service.putSps(editFormData).subscribe({
  //         next: (data) => {
  //           if (data && data.result && data.returnStatus == 1) {
  //             alert(data.returnMsg);
  //             this._dialogRef.close(true);
  //           } else {
  //             alert(data.returnMsg);
  //             console.log(data);
  //           }
  //         }, error: (error) => {
  //           console.error(error);
  //           alert(error);
  //         }
  //       });

  //       // }); // End更新API呼叫
  //     } else {

  //       //新增使用
  //       // 表單資料放進呼叫API要傳的參數
  //       const editFormData: IPostSpsReqModel = this.addEditFormGroup.getRawValue();

  //       console.log('新增:', editFormData)
  //       editFormData.mm_id = 0;
  //       // 更新(成功在主動關Dialog)
  //       this._service.postSps(editFormData).subscribe({
  //         next: (data) => {
  //           if (data && data.result && data.returnStatus == 1) {
  //             alert(data.returnMsg);
  //             this._dialogRef.close(true);
  //           } else {
  //             alert(data.returnMsg);
  //             console.log(data);
  //           }
  //         }, error: (error) => {
  //           console.error(error);
  //           alert(error);
  //         }
  //       });

  //     } // End判斷有資料則是編輯
  //   } else {
  //     // End驗證表單條件是否正確
  //     alert('欄位資料錯誤')
  //     // let errors: string = ''; // 初始化為空字串
  //     // for (const control in this.addEditFormGroup.controls) {
  //     //   errors += this.addEditFormGroup.get(control)?.errors;

  //     // }
  //     // alert(errors);
  //   }
  // }

  onFormClose(): void {
    console.log('測試關閉')
    this._dialogRef.close(false);
  }
  //End Section 4: Component Mathod Setup=======================================================================================

  //Start Section 5: Component unique Mathod Setup=======================================================================================
  //Start Section 5: Component unique Mathod Setup=======================================================================================
}
