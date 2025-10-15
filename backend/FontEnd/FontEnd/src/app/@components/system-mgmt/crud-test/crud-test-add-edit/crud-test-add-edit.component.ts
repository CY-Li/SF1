import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogActions, MatDialogContent, MatDialogRef, MatDialogTitle } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { CommonModule } from '@angular/common';
import { IPutSpsReqModel } from '../../sps/sps.model';
import { SpsService } from '../../sps/sps.service';

@Component({
  selector: 'app-crud-test-add-edit',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatDialogActions,
    MatDialogTitle,
    MatDialogContent
  ],
  templateUrl: './crud-test-add-edit.component.html',
  styleUrl: './crud-test-add-edit.component.scss'
})
export class CrudTestAddEditComponent implements OnInit {
  //Start Section 1: Form Setup=======================================================================================
  // 定義頁面Form表單用
  addEditFormGroup: FormGroup;
  // 初始化Form表單條件用
  initFormGroup() {
    // 定義頁面Form表單查詢條件
    return this._formBuilder.group({
      sps_id: this._formBuilder.control(''),
      sps_code: this._formBuilder.control('', Validators.maxLength(25)),
      sps_name: this._formBuilder.control('', Validators.maxLength(25)),
      sps_description: this._formBuilder.control('', Validators.maxLength(100)),
      sps_parameter01: this._formBuilder.control('', Validators.maxLength(30)),
      sps_parameter02: this._formBuilder.control('', Validators.maxLength(30)),
      sps_parameter03: this._formBuilder.control('', Validators.maxLength(30)),
      sps_parameter04: this._formBuilder.control('', Validators.maxLength(30)),
      sps_parameter05: this._formBuilder.control('', Validators.maxLength(30)),
      sps_parameter06: this._formBuilder.control('', Validators.maxLength(30)),
    });
  }


  // currentItem: any = null;
  // editFormData: any = {};
  //End Section 1: Form Setup=======================================================================================

  //Start Section 2: Table Setup=======================================================================================
  //End Section 2: Table Setup=======================================================================================


  //Start Section 3: Component Setup=======================================================================================

  isEditing: boolean = false; //如果是編輯用來控制欄位不可修改
  isReadonly: boolean = true; //用來判斷頁面新增、修改時是否不可輸入(系統自帶)

  constructor(
    private _formBuilder: FormBuilder, // 表單建立用
    private _dialogRef: MatDialogRef<CrudTestAddEditComponent>, // 關閉dialog，新增、編輯用
    @Inject(MAT_DIALOG_DATA) public data: any, // 主頁面傳入資料用
    private _spsService: SpsService // 後端API服務呼叫用
  ) {
    if (data.formData) this.isEditing = true; //有資料則是編輯
    this.addEditFormGroup = this.initFormGroup() // 頁面表單條件初始化用
  }

  ngOnInit(): void {
    //將傳入該Dialog資料放入表單中
    this.addEditFormGroup.patchValue(this.data.formData);
    if (this.data.isView) this.addEditFormGroup.disable();
  }

  //End Section 3: Component Setup=======================================================================================

  //Start Section 4: Component Mathod Setup=======================================================================================

  // 表單送出呼叫用
  onFormSubmit(): void {
    // 驗證表單條件是否正確
    if (this.addEditFormGroup.valid) {
      // 表單資料放進呼叫API要傳的參數
      const editFormData: IPutSpsReqModel = this.addEditFormGroup.getRawValue();

      // 判斷有資料則是編輯
      if (this.data.formData) {
        console.log('更新:', editFormData)

        // 更新(成功在主動關Dialog)
        // this._spsService.putSps(editFormData).subscribe({
        //   next: (data) => {
        //     if (data && data.result && data.returnStatus == 1) {
        //       alert(data.returnMsg);
        //       this._dialogRef.close(true);
        //     } else {
        //       alert(data.returnMsg);
        //       console.log(data);
        //     }
        //   }, error: (error) => console.error(error)
        // });

        // }); // End更新API呼叫
      } else {

        //新增使用


      } // End判斷有資料則是編輯
    } else {
      // End驗證表單條件是否正確
      alert('欄位資料錯誤')
      // let errors: string = ''; // 初始化為空字串
      // for (const control in this.addEditFormGroup.controls) {
      //   errors += this.addEditFormGroup.get(control)?.errors;

      // }
      // alert(errors);
    }
  }

  onFormClose(): void {
    console.log('測試關閉')
    this._dialogRef.close(false);
  }
  //End Section 4: Component Mathod Setup=======================================================================================
}
