import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogActions, MatDialogContent, MatDialogRef, MatDialogTitle } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatCardModule } from '@angular/material/card';
import { CommonModule } from '@angular/common';
import { AnnBoardService } from '../ann-board.service';
import { IpostAnnouncementBoard, IputAnnouncementBoard } from '../ann-board.model';
import { lengthValidator } from '../../../../@shared/valid/customize-validator.service';
import { MatIconModule } from '@angular/material/icon';
import { provideNativeDateAdapter } from '@angular/material/core';


@Component({
  selector: 'app-ann-board-add-edit',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatCardModule,
    MatDatepickerModule,
    MatIconModule,
    MatDialogActions,
    MatDialogTitle,
    MatDialogContent
  ],
  providers: [provideNativeDateAdapter()],
  templateUrl: './ann-board-add-edit.component.html',
  styleUrl: './ann-board-add-edit.component.scss'
})
export class AnnBoardAddEditComponent {
  //Start Section 1: Form Setup=======================================================================================
  // 定義頁面Form表單用
  addEditFormGroup: FormGroup;
  // 初始化Form表單條件用
  initFormGroup() {
    // 定義頁面Form表單查詢條件
    return this._formBuilder.group({
      ab_id: this._formBuilder.control(''),
      ab_title: this._formBuilder.control('', [Validators.required, Validators.maxLength(30)]),
      ab_content: this._formBuilder.control('', Validators.maxLength(300)),
      // ab_image: null,
      // ab_image_url: this._formBuilder.control('', Validators.maxLength(100)),
      ab_status: this._formBuilder.control('', [Validators.required, Validators.pattern('^(?:10|20)?$')]),
      ab_datetime: this._formBuilder.control('',[Validators.required]),

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
    private _dialogRef: MatDialogRef<AnnBoardAddEditComponent>, // 關閉dialog，新增、編輯用
    @Inject(MAT_DIALOG_DATA) public data: any, // 主頁面傳入資料用
    private _service: AnnBoardService // 後端API服務呼叫用
  ) {
    this.isView = this.data.isView;
    if (data.formData) this.isEditing = true; //有資料則是編輯
    this.addEditFormGroup = this.initFormGroup() // 頁面表單條件初始化用

    if (!this.isEditing) {
      this.addEditFormGroup.get('ab_datetime')?.setValue(new Date());
    }
  }

  ngOnInit(): void {
    //將傳入該Dialog資料放入表單中
    if (this.data.formData) {
      this.addEditFormGroup.patchValue(this.data.formData);
    }

    if (this.data.formData.ab_datetime) {
      this.addEditFormGroup.get('ab_datetime')?.setValue(new Date(this.data.formData.ab_datetime));
    }

    if (this.isView) this.addEditFormGroup.disable();
  }

  //End Section 3: Component Setup=======================================================================================

  //Start Section 4: Component Mathod Setup=======================================================================================

  // 表單送出呼叫用
  onFormSubmit(): void {

    // 驗證表單條件是否正確
    if (this.addEditFormGroup.valid) {

      // 判斷有資料則是編輯
      if (this.data.formData) {
        // 表單資料放進呼叫API要傳的參數
        // const tempFormData: IputAnnouncementBoard = this.addEditFormGroup.getRawValue();
        const tempFormData = this.addEditFormGroup.getRawValue();
        let editFormData = new FormData();

        for (const key in tempFormData) {
          if (tempFormData.hasOwnProperty(key)) {
            if (key === 'ab_datetime' && tempFormData[key]) {
              editFormData.append(key, new Date(tempFormData[key]).toDateString());
            } else {
              editFormData.append(key, tempFormData[key]);
            }
            // editFormData.append(key, tempFormData[key]);
          }
        }

        if (this.images) {
          editFormData.append('file', this.images)
        }
        console.log('更新:', editFormData)
        editFormData.append('mm_id', '0');

        // // 更新(成功在主動關Dialog)
        this._service.putAnnouncementBoard(editFormData).subscribe({
          next: (data) => {
            if (data && data.result && data.returnStatus == 1) {
              alert(data.returnMsg);
              this._dialogRef.close(true);
            } else {
              alert(data.returnMsg);
              console.log(data);
            }
          }, error: (error) => {
            console.error(error);
            alert(error);
          }
        });

        // }); // End更新API呼叫
      } else {

        // Start 新增
        // 表單資料放進呼叫API要傳的參數
        // const editFormData: IpostAnnouncementBoard = this.addEditFormGroup.getRawValue();
        const tempFormData = this.addEditFormGroup.getRawValue();
        let editFormData = new FormData();

        for (const key in tempFormData) {
          if (tempFormData.hasOwnProperty(key)) {
            if (key === 'ab_datetime' && tempFormData[key]) {
              editFormData.append(key, new Date(tempFormData[key]).toISOString());
            } else {
              editFormData.append(key, tempFormData[key]);
            }
            // editFormData.append(key, tempFormData[key]);
          }
        }

        if (this.images) {
          editFormData.append('file', this.images)
        } else {
          alert('新增必須有圖片');
          return;
        }
        console.log('更新:', editFormData)
        editFormData.append('mm_id', '0');

        // 更新(成功在主動關Dialog)
        this._service.postAnnouncementBoard(editFormData).subscribe({
          next: (data) => {
            if (data && data.result && data.returnStatus == 1) {
              alert(data.returnMsg);
              this._dialogRef.close(true);
            } else {
              alert(data.returnMsg);
              console.log(data);
            }
          }, error: (error) => {
            console.error(error);
            alert(error);
          }
        });
        // End 新增
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

  //Start Section 5: Component unique Mathod Setup=======================================================================================

  imageSrc: string | ArrayBuffer | null = null;
  // images: File[] = []
  images: File | null = null; // 或者使用 undefined

  onFileChange(event: any) {
    const file = event.target.files[0];
    this.images = file;
    console.log(file);
    if (file) {
      const reader = new FileReader();
      reader.onload = () => {
        this.imageSrc = reader.result; // 將讀取的結果設定為圖片源
      };
      reader.readAsDataURL(file);
    }
  }

  getFormImage(event: any) {
    // 檢查 ab_image_url 是否存在且不為空
    // const imageUrl = this.data.formData.ab_image_url;
    // return imageUrl ? imageUrl : 'default-image-url.jpg'; // 替換為您的預設圖片URL
    return this.data.formData.ab_image_url
  }
  //Start Section 5: Component unique Mathod Setup=======================================================================================
}
